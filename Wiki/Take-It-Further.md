## Scenario 1

Customize the app to have a different name depending on the team it is installed in - For ex: IT aka Contoso IT Support.
 
**Suggested Solution:** Please follow below mentioned steps to configure the app to be used for different domains:

**Code Changes:**

- Change the text references in the associated resource(Strings.resx) file from FAQ Plus to the domain on which it should cater to.

- Update the app name, description, tab name and other details in the associated manifest JSON files for end-user and experts team respectively.

- Change the welcome message as desired in the configurator web app.