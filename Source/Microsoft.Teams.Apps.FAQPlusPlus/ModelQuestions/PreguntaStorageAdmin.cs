using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    public class PreguntaStorageAdmin
    {
        // Declarar el objeto storage.
        private CloudStorageAccount storageAccounts;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreguntaStorageAdmin"/> class.
        /// </summary>
        public void ModeloPreguntasGuardar()
        {
            // Crear el objeto storage a través de la cadena de conexión.
            this.storageAccounts = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=usk4trik6dp4a;AccountKey=6pyOUZjEOnzHykzNheiCESC8WSDtRuv0hBiU6IKNuGl5lJfBaKfwzSyLE/nnJs8zZ1jXkyQajrDfas3y9AuruA==;EndpointSuffix=core.windows.net");
        }

        public CloudTable CrearTablaAzureStorage()
        {
            CloudTableClient tableAnimal = this.storageAccounts.CreateCloudTableClient();
            CloudTable tabla = tableAnimal.GetTableReference("Tickets");
            tabla.CreateIfNotExistsAsync();
            return tabla;
        }

        public void GuardarPreguntaTabla(string PartKey, String Title, String RequesterName, String UserQuestion)
        {
            if (Title.ToString().ToLower().Trim() != "siguiente" && Title.ToString().ToLower().Trim() != "Si, Finalizar" && Title.ToString().ToLower().Trim() != "No, Remitir a un asesor de Houston")
            {
                CloudTable tabla = this.CrearTablaAzureStorage();
                PreguntasGuardar preguntas = new PreguntasGuardar();
                preguntas.PartitionKey = PartKey;
                preguntas.Title = Title;
                preguntas.RequesterName = RequesterName;
                preguntas.UserQuestion = UserQuestion;
                preguntas.RowKey = Guid.NewGuid().ToString();
                TableOperation insertOperation = TableOperation.Insert(preguntas);
                tabla.ExecuteAsync(insertOperation);
            }
        }
    }
}
