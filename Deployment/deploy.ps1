<#
    .SYNOPSIS
    Automate deployment of FAQ Plus app template
    .NOTES
    This involves validating for Azure region, resources names, creating AAD apps, deploying ARM templates and generating app manifests.
    Both AD applications require User.Read application level permission consented in Microsoft Graph.
    {
        "id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
        "type": "Scope"
    }

    .EXAMPLE
    .\deploy.ps1 <<TenantId>> <<SubscriptionId>> <<SubscriptionTenantId>> <<AzureRegionName>> <<ResourceGroupName>> <<BaseAppName>> <<BaseResourceName>> <<ConfigAdminUPNList>> <<CompanyName>> <<WebsiteUrl>> <<PrivacyUrl>> <<TermsOfUseUrl>>
    
    .\deploy.ps1 98f3ece2-3a5a-428b-aa4f-4c41b3f6eef0 22f602c4-1b8f-46df-8b73-45d7bdfbf58e westus2 ContosoResourceGroup "FAQ Plus" FAQPlus "admin@contoso.onmicrosoft.com;user1@contoso.onmicrosoft.com" "Contoso" "http://www.contoso.com/" "http://www.contoso.com/privacy" "http://www.contoso.com/terms"


-----------------------------------------------------------------------------------------------------------------------------------
Script name : deploy.ps1
Version : 1.0
Dependencies : Azure CLI, AzureADPreview, AZ, WriteAscii
-----------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
Version Changes:
Date:       Version: Changed By:     Info:
-----------------------------------------------------------------------------------------------------------------------------------
DISCLAIMER
   THIS CODE IS SAMPLE CODE. THESE SAMPLES ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND.
   MICROSOFT FURTHER DISCLAIMS ALL IMPLIED WARRANTIES INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES
   OF MERCHANTABILITY OR OF FITNESS FOR A PARTICULAR PURPOSE. THE ENTIRE RISK ARISING OUT OF THE USE OR
   PERFORMANCE OF THE SAMPLES REMAINS WITH YOU. IN NO EVENT SHALL MICROSOFT OR ITS SUPPLIERS BE LIABLE FOR
   ANY DAMAGES WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS, BUSINESS
   INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR OTHER PECUNIARY LOSS) ARISING OUT OF THE USE OF OR
   INABILITY TO USE THE SAMPLES, EVEN IF MICROSOFT HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
   BECAUSE SOME STATES DO NOT ALLOW THE EXCLUSION OR LIMITATION OF LIABILITY FOR CONSEQUENTIAL OR
   INCIDENTAL DAMAGES, THE ABOVE LIMITATION MAY NOT APPLY TO YOU.
#>

<# Valid Azure locations
Check the list from here
https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor
#>

    # Validate URL with https prefix
    function ValidateSecureUrl{
        param(
            [Parameter(Mandatory = $true)] [string] $url
        )
        # Url with https prefix REGEX matching
        return ($url -match "https:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)")
    }

    # Validate all URL parameters in JSON file
    function ValidateUrlParameters{
        $isValidUrl = $true
        $isValidUrl = $isValidUrl -and (ValidateSecureUrl $parameters.WebsiteUrl.Value)
        $isValidUrl = $isValidUrl -and (ValidateSecureUrl $parameters.PrivacyUrl.Value)
        $isValidUrl = $isValidUrl -and (ValidateSecureUrl $parameters.TermsOfUseUrl.Value)
        return $isValidUrl
    }

    # Test for availability of Azure resources
    function Test-AzNameAvailability {
        param(
            [Parameter(Mandatory = $true)] [string] $AuthorizationToken,
            [Parameter(Mandatory = $true)] [string] $Name,
            [Parameter(Mandatory = $true)] [ValidateSet(
                'ApiManagement', 'KeyVault', 'ManagementGroup', 'Sql', 'StorageAccount', 'WebApp', 'CognitiveService')]
            $ServiceType
        )
 
        $uriByServiceType = @{
            ApiManagement   = 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.ApiManagement/checkNameAvailability?api-version=2019-01-01'
            KeyVault        = 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.KeyVault/checkNameAvailability?api-version=2019-09-01'
            ManagementGroup = 'https://management.azure.com/providers/Microsoft.Management/checkNameAvailability?api-version=2018-03-01-preview'
            Sql             = 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Sql/checkNameAvailability?api-version=2018-06-01-preview'
            StorageAccount  = 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Storage/checkNameAvailability?api-version=2019-06-01'
            WebApp          = 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Web/checkNameAvailability?api-version=2020-06-01'
            CognitiveService= 'https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.CognitiveServices/checkDomainAvailability?api-version=2017-04-18'
        }

        $typeByServiceType = @{
            ApiManagement   = 'Microsoft.ApiManagement/service'
            KeyVault        = 'Microsoft.KeyVault/vaults'
            ManagementGroup = '/providers/Microsoft.Management/managementGroups'
            Sql             = 'Microsoft.Sql/servers'
            StorageAccount  = 'Microsoft.Storage/storageAccounts'
            WebApp          = 'Microsoft.Web/sites'
            CognitiveService= 'Microsoft.CognitiveServices/accounts'
        }

        $uri = $uriByServiceType[$ServiceType] -replace ([regex]::Escape('{subscriptionId}')), $parameters.SubscriptionId.Value
        $nameProperty = If ($ServiceType -eq 'CognitiveService') {"subdomainName"} Else {"name"}
        $body = '"{0}": "{1}", "type": "{2}"' -f $nameProperty, $Name, $typeByServiceType[$ServiceType]

        $response = (Invoke-WebRequest -Uri $uri -Method Post -Body "{$body}" -ContentType "application/json" -Headers @{Authorization = $AuthorizationToken } -UseBasicParsing).content
        $response | ConvertFrom-Json |
        Select-Object @{N = 'Name'; E = { $Name } }, @{N = 'Type'; E = { $ServiceType } }, @{N = 'Available'; E = { $_ | Select-Object -ExpandProperty *available } }, Reason, Message
    }

    # Get Azure access token for current user
    function Get-AccessTokenFromCurrentUser {
        try{
        $azContext = Get-AzContext
        $azProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
        $profileClient = New-Object -TypeName Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient -ArgumentList $azProfile
        $token = $profileClient.AcquireAccessToken($azContext.Subscription.TenantId)
        ('Bearer ' + $token.AccessToken)
    }        
        catch {
            throw
        }
    }        

    # Check that the provided location is a valid Azure location
    function ValidateAzureLocation {
        Write-Host "Validate selected region name & supported features..."

        $locations = Get-AzLocation
        
        $location = $parameters.Location.Value.Replace(" ", "").ToLower()

        $azureLocation = $locations | Where-Object Location -eq $location

        # Validate that the location exists
        if ($null -eq $azureLocation) {
            throw "Invalid Azure Location. Please provide a valid location. See this list - https://azure.microsoft.com/en-gb/global-infrastructure/locations/"
        }

        # Validate that the location supports required services
        $providers = $azureLocation.Providers
        if ($null -eq ($providers | where {$_.ToLower() -match 'microsoft.insights'})) {
            throw "The selected Azure location does not support the 'Application Insights' resource type. Please use another location from this list - https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor"
        }

        if ($null -eq ($providers | where {$_.ToLower() -match 'microsoft.cognitiveservices'})) {
            throw "The selected Azure location does not support the 'Cognitive Services' resource type. Please use another location from this list - https://azure.microsoft.com/en-us/global-infrastructure/services/?products=logic-apps,cognitive-services,search,monitor"
        }
    }

    # Check if resource type/name exist in the Azure cloud
    function ValidateResourceNames {
        param(
                [Parameter(Mandatory = $true)] $resourceInfo
            )

        if($resourceInfo.ServiceType -eq "ApplicationInsights"){
            if ($null -eq (Get-AzApplicationInsights | Where-Object Name -eq $resourceInfo.Name)) {
                Write-Host "Application Insights resource ($($resourceInfo.Name)) is available." -ForegroundColor Green
                return $true
            }
            else{
                Write-Host "Application Insights resource ($($resourceInfo.Name)) is not available." -ForegroundColor Yellow
                return $false
            }
        }
        else{
            $availabilityResult = $null
            $availabilityResult = Test-AzNameAvailability @resourceInfo -ErrorAction Stop
        
            if ($availabilityResult.Available) {
                Write-Host "Resource: $($resourceInfo.Name) of type $($resourceInfo.ServiceType) is available." -ForegroundColor Green
                return $true
            }
            else{
                Write-Host "Resource $($resourceInfo.Name) is not available." -ForegroundColor Yellow
                Write-Host $availabilityResult.message -ForegroundColor Yellow
                return $false
            }
        }
    }

    # Check that the resources names does not already exist and ensure names are valid
    function ValidateResourcesNames {
        Write-Host "Checking for the availability of resources..."

        $authorizationToken = Get-AccessTokenFromCurrentUser -ErrorAction Stop

        $resources =@(@{
            Name               = $parameters.BaseResourceName.Value
            ServiceType        = 'WebApp'
            AuthorizationToken = $authorizationToken
        },
        @{
            Name               = $parameters.BaseResourceName.Value + '-config'
            ServiceType        = 'WebApp'
            AuthorizationToken = $authorizationToken
        },
        @{
            Name               = $parameters.BaseResourceName.Value + '-function'
            ServiceType        = 'WebApp'
            AuthorizationToken = $authorizationToken
        },
        @{
            Name               = $parameters.BaseResourceName.Value + '-qnamaker'
            ServiceType        = 'WebApp'
            AuthorizationToken = $authorizationToken
        },
        @{
            Name               = $parameters.BaseResourceName.Value
            ServiceType        = 'CognitiveService'
            AuthorizationToken = $authorizationToken
        },
        @{
            Name               = $parameters.BaseResourceName.Value
            ServiceType        = 'ApplicationInsights'
        },
        @{
            Name               = $parameters.BaseResourceName.Value + '-config'
            ServiceType        = 'ApplicationInsights'
        },
        @{
            Name               = $parameters.BaseResourceName.Value + '-qnamaker'
            ServiceType        = 'ApplicationInsights'
        })

        $allResourcesAvailable = $true
        foreach($resource in $resources){
            $isResourceNameAvailable = ValidateResourceNames $resource -ErrorAction Stop
            $allResourcesAvailable = $allResourcesAvailable -and $isResourceNameAvailable
        }

        if(!$allResourcesAvailable){
            $confirmationTitle    = "Some of the resource types names already exist. If you proceed, this will update the existing resources."
            $confirmationQuestion = "Are you sure you want to proceed?"
            $confirmationChoices  = "&Yes", "&No" # 0 = Yes, 1 = No
            
            $updateDecision = $Host.UI.PromptForChoice($confirmationTitle, $confirmationQuestion, $confirmationChoices, 1)
            if ($updateDecision -eq 0) {
                return $true
            }
            else {
                return $false
            }
        }
    }


    # Gets the Azure AD app
    function GetAzureADApp {
        param ($appName)

        $app = az ad app list --filter "displayName eq '$appName'" | ConvertFrom-Json

        return $app

    }

    function CreateAzureADApp {
        param(
                [Parameter(Mandatory = $true)] [string] $AppName,
                [Parameter(Mandatory = $false)] [bool] $MultiTenant,
                [Parameter(Mandatory = $false)] [bool] $AllowImplicitFlow,
                [Parameter(Mandatory = $false)] [string[]] $RedirectUris,
                [Parameter(Mandatory = $false)] [bool] $ResetAppSecret = $false
            )

        try {
            Write-Host "`r`n### AZURE AD APP CREATION ($appName) ###"

            # Check if the app already exists - script has been previously executed
            $app = GetAzureADApp $appName

            if (-not ([string]::IsNullOrEmpty($app))) {

                # Update Azure AD app registration using CLI
                $confirmationTitle    = "The Azure AD app '$appName' already exists. If you proceed, this will update the existing app configuration."
                $confirmationQuestion = "Are you sure you want to proceed?"
                $confirmationChoices  = "&Yes", "&No" # 0 = Yes, 1 = No
                
                $updateDecision = $Host.UI.PromptForChoice($confirmationTitle, $confirmationQuestion, $confirmationChoices, 1)
                if ($updateDecision -eq 0) {
                    Write-Host "Updating the existing app..." -ForegroundColor Yellow

                    az ad app update --id $app.appId --available-to-other-tenants $MultiTenant --oauth2-allow-implicit-flow $AllowImplicitFlow --required-resource-accesses './AdAppManifest.json'

                    Write-Host "Waiting for app update to finish..."
    
                    Start-Sleep -s 10
    
                    Write-Host "Azure AD App ($appName) is updated." -ForegroundColor Green
    
                } else {
                    Write-Host "Deployment cancelled. Please use a different name for the Azure AD app and try again." -ForegroundColor Yellow
                    return $null
                }
            } 
            else {
                # Create the app
                Write-Host "Creating Azure AD App - ($appName)..."

                # Create Azure AD app registration using CLI
                az ad app create --display-name $appName --end-date '2299-12-31T11:59:59+00:00' --available-to-other-tenants $MultiTenant --oauth2-allow-implicit-flow $AllowImplicitFlow --required-resource-accesses './AdAppManifest.json'

                Write-Host "Waiting for app creation to finish..."

                Start-Sleep -s 10

                Write-Host "Azure AD App ($appName) is created." -ForegroundColor Green

            }

            $app = GetAzureADApp $appName
            
            $appSecret = $null;
            if($ResetAppSecret){
                Write-Host "Update app secret..."
                $appSecret = (az ad app credential reset --id $app.appId --append | ConvertFrom-Json).password;
            }

            if($null -ne $RedirectUris){
                Write-Host "Update reply urls..."
                az ad app update --id $app.appId --reply-urls @RedirectUris
            }

            $consentErrorMessage = "Current user does not have the privilege to consent the `"User.Read`" permission on this app. Please ask your tenant administrator to consent."
            # Grant admin consent for app registration required permissions using CLI
            az ad app permission admin-consent --id $app.appId
            Write-Host "Waiting for admin consent to finish..."
            if (0 -ne $LastExitCode) {
                Write-Host $consentErrorMessage -ForegroundColor Yellow
                [Console]::ResetColor()
            }
            else{
                Write-Host "admin consent has been granted." -ForegroundColor Green
            }

            Write-Host "### AZURE AD APP ($appName) CREATION & CONFIGURATION FINISHED ###" -ForegroundColor Green
            return @{
                appId = $app.appId
                appSecret = $appSecret
            }
        }
        catch {
            $errorMessage = $_.Exception.Message
            Write-Host "Error occured while creating an Azure AD App: $errorMessage" -ForegroundColor Red
        }
        return $null
    }

    function GetResourceName {
        param (
            [Parameter(Mandatory = $true)] [string] $resourceId
        )
        # ResourceId look like this /subscriptions/*****/resourcegroups/****/providers/****/sites/***ResourceName***/***childComponent***/***
        $resourceParentIndex = $resourceId.indexOf('/sites/')
        if($resourceParentIndex -eq -1){
            return $null
        }
        $resourceNameStartIndex = $resourceParentIndex + 7 # 7 is length of /sites/
        return $resourceId.Substring($resourceNameStartIndex, $resourceId.indexOf('/', $resourceNameStartIndex) - $resourceNameStartIndex)
    }

    # Collect error logs after failed ARM deployment
    function CollectARMDeploymentLogs {
        $logsPath = '.\DeploymentLogs'
        $activityLogPath = "$logsPath\activity_log.log"
        $deploymentLogPath = "$logsPath\deployment_operation.log"

        $logsFolder = New-Item -ItemType Directory -Force -Path $logsPath

        az deployment operation group list --resource-group $parameters.ResourceGroupName.Value --name azuredeploy --query "[?properties.provisioningState=='Failed'].properties.statusMessage.error" | Set-Content $deploymentLogPath

        $activityLog = $null
        $retryCount = 5
        DO
        {
            Write-Host "Collecting deployment logs..."

            # Wait for async logs to persist
            Start-Sleep -s 30

            # Returns empty [] if logs are not available yet
            $activityLog = az monitor activity-log list -g $parameters.ResourceGroupName.Value --caller $userAlias --status Failed --offset 30m

            $retryCount--

        } While (($activityLog.Length -lt 3) -and ($retryCount -gt 0))

        $activityLog | Set-Content $activityLogPath

        # collect web apps deployment logs
        $activityLogErrors = ($activityLog | ConvertFrom-Json) | Where-Object{ ($null -ne $_.resourceType) -and ($_.resourceType.value -eq "Microsoft.Web/sites/sourcecontrols") }
        $resourcesLookup = @($activityLogErrors | Select-Object resourceId, @{Name = "resourceName"; Expression = {GetResourceName $_.resourceId}})
        if($resourcesLookup.length -gt 0){
            foreach($resourceInfo in $resourcesLookup){
                if($null -ne $resourceInfo.resourceName){
                    az webapp log download --ids $resourceInfo.resourceId --log-file "$logsPath\$($resourceInfo.resourceName).zip"
                }
            }
        }
        
        # Generate zip archive and delete folder
        $compressManifest = @{
            Path= $logsPath
            CompressionLevel = "Fastest"
            DestinationPath = "logs.zip"
        }
        Compress-Archive @compressManifest -Force
        Get-ChildItem -Path $logsPath -Recurse | Remove-Item -Force -Recurse -ErrorAction Continue
        Remove-Item $logsPath -Force -ErrorAction Continue
        
        Write-Host "Deployment logs generation finished. Please share Deployment\logs.zip file with the app template team to investigate..." -ForegroundColor Yellow
    }
    
    # Deploy ARM template
    function DeployARMTemplate {
        Param(
            [Parameter(Mandatory=$true)] $botAppId,
            [Parameter(Mandatory=$true)] $configAppId,
            [Parameter(Mandatory=$true)] $appSecret,
            [Parameter(Mandatory=$true)] $userBotAppId,
            [Parameter(Mandatory=$true)] $userBotClientSecret
        )
        try { 
            if ((az group exists --name $parameters.ResourceGroupName.Value) -eq $false){
                Write-Host "Creating resource group $($parameters.ResourceGroupName.Value)..." -ForegroundColor Yellow
                az group create --name $parameters.ResourceGroupName.Value --location $parameters.Location.Value
            }
            
            # Deploy ARM templates
            Write-Host "Deploying app services, storage accounts, bot service, and cognitive services..." -ForegroundColor Yellow
            az deployment group create --resource-group $parameters.ResourceGroupName.Value --subscription $parameters.SubscriptionId.Value --template-file 'azuredeploy.json' --parameters "baseResourceName=$($parameters.BaseResourceName.Value)" "userBotClientId=$userBotAppId" "userBotClientSecret=$userBotClientSecret" "expertBotClientId=$botAppId" "expertBotClientSecret=$appSecret" "configAppClientId=$configAppId" "configAdminUPNList=$($parameters.ConfigAdminUPNList.Value)" "appDisplayName=$($parameters.AppDisplayName.Value)" "appDescription=$($parameters.AppDescription.Value)" "tenantId=$($parameters.tenantId.Value)" "appIconUrl=$($parameters.AppIconUrl.Value)" "sku=$($parameters.Sku.Value)" "planSize=$($parameters.PlanSize.Value)" "qnaMakerSku=$($parameters.QnaMakerSku.Value)" "searchServiceSku=$($parameters.SearchServiceSku.Value)" "gitRepoUrl=$($parameters.GitRepoUrl.Value)" "gitBranch=$($parameters.GitBranch.Value)" "defaultCulture=$($parameters.DefaultCulture.Value)"
            if($LASTEXITCODE -ne 0){
            #    CollectARMDeploymentLogs
                Throw "ERROR: ARM template deployment error."
            }
            Write-Host "Finished deploying resources." -ForegroundColor Green
        }
        catch {
            Write-Host "Error occured while deploying Azure resources." -ForegroundColor Red
            throw
        }
    }

    # Compress Manifest
    function CompressManifest {
        Param(
            [Parameter(Mandatory=$true)] $sourcePath,
            [Parameter(Mandatory=$true)] $destManifestFilePath,
            [Parameter(Mandatory=$true)] $destinationZipPath
        )

        # Generate zip archive 
        $compressManifest = @{
            LiteralPath= "$sourcePath\color.png", "$sourcePath\outline.png", "$sourcePath\ar.json", "$sourcePath\de.json", "$sourcePath\en.json", "$sourcePath\es.json", "$sourcePath\fr.json", "$sourcePath\he.json",
            "$sourcePath\ja.json", "$sourcePath\ko.json", "$sourcePath\pt-BR.json", "$sourcePath\ru.json", "$sourcePath\zh-CN.json", "$sourcePath\zh-TW.json", $destManifestFilePath
            CompressionLevel = "Fastest"
            DestinationPath = $destinationZipPath
            }
        Compress-Archive @compressManifest -Force

        Remove-Item $destManifestFilePath -ErrorAction Continue

        Write-Host "Package has been created under this path $(Resolve-Path $destinationZipPath)" -ForegroundColor Green
    }

    function GenerateAppManifestPackage {
        Param(
            [Parameter(Mandatory=$true)] [ValidateSet(
                'SME', 'EndUser')] $manifestFolder,
            [Parameter(Mandatory=$true)] $botIdKey,
            [Parameter(Mandatory=$true)] $botIdValue,
            [Parameter(Mandatory=$true)] $isLegacy
        )

        Write-Host "Generating package for $manifestType..."

        $suffix = $manifestFolder.ToLower()
        If ($true -eq $isLegacy) {
            $suffix = 'legacy'
        }
        $azureDomainBase = 'azurewebsites.net'
        $sourcePath = "..\Manifest\$manifestFolder"
        $sourceManifestPath = "$sourcePath\manifest_$suffix.json"
        $destManifestFilePath = "$sourcePath\manifest.json"
        $destinationZipPath = "..\Manifest\faqplus-$suffix.zip"
        
        if(!(Test-Path $sourceManifestPath)){
            throw "$sourceManifestPath does not exist. Please make sure you download the full app template source."
        }

        copy-item -path $sourceManifestPath -destination $destManifestFilePath -Force

        # Replace merge fields with proper values in manifest file and save
        $mergeFields = @{
            '<<companyName>>' = $parameters.CompanyName.Value 
            '<<websiteUrl>>' = $parameters.WebsiteUrl.Value
            '<<privacyUrl>>' = $parameters.PrivacyUrl.Value
            '<<termsOfUseUrl>>' = $parameters.TermsOfUseUrl.Value
            '<<appDomain>>' = "$($parameters.BaseResourceName.Value).$azureDomainBase"
        }

        $mergeFields.Add("<<$botIdKey>>", $botIdValue)
        $appManifestContent = Get-Content $destManifestFilePath
        foreach ($mergeField in $mergeFields.GetEnumerator()) {
            $appManifestContent = $appManifestContent.replace($mergeField.Name, $mergeField.Value)
        }
        $appManifestContent | Set-Content $destManifestFilePath -Force

        # Generate zip archive 
        CompressManifest $sourcePath $destManifestFilePath $destinationZipPath
    }

    # Check for presence of Azure CLI
    If (-not (Test-Path -Path "C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2")) {
        Write-Host "AZURE CLI NOT INSTALLED!`nPLEASE INSTALL THE CLI FROM https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest and re-run this script in a new PowerShell session" -ForegroundColor Red
        break
    }

    # # Install required modules
    Write-Host "Checking for required modules..." -ForegroundColor Yellow
    if (-not (Get-Module -ListAvailable -Name "Az")) {
        Write-Host "Installing AZ module..." -ForegroundColor Yellow
        Install-Module Az -AllowClobber -Scope CurrentUser
    } 
    if (-not (Get-Module -ListAvailable -Name "AzureADPreview")) {
        Write-Host "Installing AzureADPreview module..." -ForegroundColor Yellow
        Install-Module AzureADPreview -Scope CurrentUser
    } 
    if (-not (Get-Module -ListAvailable -Name "WriteAscii")) {
        Write-Host "Installing WriteAscii module..." -ForegroundColor Yellow
        Install-Module WriteAscii -Scope CurrentUser
    } 

    # Loading Parameters from JSON meta-data file
    $parametersListContent = Get-Content '.\parameters.json' -ErrorAction Stop
    $missingRequiredParameter = $parametersListContent | %{$_ -match '<<value>>'}
    If($missingRequiredParameter -contains $true){
        Write-Host "Some required parameters are missing values. Please replace all <<value>> occurrences in parameters.json file with correct values." -ForegroundColor Red
        Exit
    }

    # Parse & assign parameters
    $parameters = $parametersListContent | ConvertFrom-Json

    # Validate Https Urls parameters
    if(!(ValidateUrlParameters)){
        Write-Host "WebsiteUrl, PrivacyUrl, TermsOfUseUrl parameters must be in correct format and start with https:// prefix. Please correct values in parameters.json file." -ForegroundColor Red
        Exit
    }

    Write-Ascii -InputObject "FAQ Plus App Template" -ForegroundColor Magenta

    Write-Host "### DEPLOYMENT SCRIPT STARTED ###" -ForegroundColor Magenta

    # Initialise connections - Azure Az/CLI
    Write-Host "Login with your Azure subscription account. Launching Azure sign-in window..."
    Connect-AzAccount -Subscription $parameters.subscriptionId.Value -ErrorAction Stop
    $user = az login --tenant $parameters.subscriptionTenantId.value
    if($LASTEXITCODE -ne 0){
        return
    }
    $userAlias = ($user | ConvertFrom-Json).user.name

    # Validate region & supported features
    Write-Host "Initial Validations..."
    ValidateAzureLocation

    # Validate resources names existence
    $shouldProceed = ValidateResourcesNames -ErrorAction Stop
    # User cancelled operation.
    If($false -eq $shouldProceed){
        Exit
    }

    if ($parameters.subscriptionTenantId.Value -ne $parameters.tenantId.Value)
    {
        # Log out of Azure subscription account to avoid tokens caching
        az logout
        Disconnect-AzAccount

        # Log in to the tenant where thia app template is going to be used.
        Write-Host "Please login to the tenant where this app template will be used in Microsoft Teams..."
        az login --tenant $parameters.tenantId.value --allow-no-subscriptions
    }

    # Create expert bot app (Name, App Secret, Multi-Organization support, Enable ID Tokens)
    $expertBotAppId = $null
    $expertBotClientSecret = $null
    $expertBotApp = @{
        AppName = $parameters.BaseAppName.Value + "-expert"
        ResetAppSecret = $true
        MultiTenant = $true
        AllowImplicitFlow = $false
        RedirectUris = $null
    }
    $expertBotApp = CreateAzureADApp @expertBotApp
    $expertBotApp = If ($expertBotApp -is [array]) {$expertBotApp[-1]} Else {$expertBotApp}
    # User cancelled operation or error occurred.
    If($null -eq $expertBotApp){
        Exit
    }
    else{
        $expertBotAppId = $expertBotApp.appId;
        $expertBotClientSecret = $expertBotApp.appSecret
    }

    # Create user bot app (Name, App Secret, Multi-Organization support, Enable ID Tokens)
    $userBotAppId = $null
    $userBotClientSecret = $null
    $userBotApp = @{
        AppName = $parameters.BaseAppName.Value + "-user"
        ResetAppSecret = $true
        MultiTenant = $true
        AllowImplicitFlow = $false
        RedirectUris = $null
    }
    $userBotApp = CreateAzureADApp @userBotApp
    $userBotApp = If ($userBotApp -is [array]) {$userBotApp[-1]} Else {$userBotApp}
    # User cancelled operation or error occurred.
    If($null -eq $userBotApp){
        Exit
    }
    else{
        $userBotAppId = $userBotApp.appId;
        $userBotClientSecret = $userBotApp.appSecret
    }
    
    # Create config app (Name, Single-Organization support, Enable ID Tokens, and Add Reply-Urls)
    $configAppId = $null
    $azureDomainBase = 'azurewebsites.net'
    $configAppUrl = "https://$($parameters.BaseResourceName.Value)-config.$azureDomainBase"
    $botConfigApp = @{
        AppName = "$($parameters.BaseAppName.Value) Configuration"
        MultiTenant = $false
        AllowImplicitFlow = $true
        RedirectUris = @($configAppUrl, ($configAppUrl + '/signin'), ($configAppUrl + '/configuration'))
    }
    $configApp = CreateAzureADApp @botConfigApp
    $configApp = If ($configApp -is [array]) {$configApp[-1]} Else {$configApp}
    # User cancelled operation or error occurred.
    If($null -eq $configApp){
        Exit
    }
    else{
        $configAppId = $configApp.appId
    }

    if ($parameters.subscriptionTenantId.Value -ne $parameters.tenantId.Value)
    {
        az logout

        Write-Host "Login with your Azure subscription account. Launching Azure sign-in window..."
        Connect-AzAccount -Subscription $parameters.subscriptionId.Value -ErrorAction Stop
        az login --tenant $parameters.subscriptionTenantId.value
        az account set --subscription $parameters.subscriptionId.Value  
    }
    
    # Deploy the other resources in ARM template
    DeployARMTemplate $expertBotAppId $configAppId $expertBotClientSecret $userBotAppId $userBotClientSecret -ErrorAction Stop

    # Generate Apps manifests
    GenerateAppManifestPackage "EndUser" "userBotId" $userBotAppId $false
    GenerateAppManifestPackage "SME" "expertBotId" $expertBotAppId $false
    If ($true -eq $parameters.IsMigration.Value) {
        GenerateAppManifestPackage "SME" "userBotId" $userBotAppId $true
    }

    # Log out to avoid tokens caching
    az logout
    Disconnect-AzAccount

    # Open manifest folder
    Invoke-Item ..\Manifest\

    Write-Ascii -InputObject "DEPLOYMENT SUCCEEDED." -ForegroundColor Green