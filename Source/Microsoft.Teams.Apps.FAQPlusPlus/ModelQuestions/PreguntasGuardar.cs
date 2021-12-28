using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    /// <summary>
    /// Guarda la pregunta generada por el bot y que no pertenece a un ticket.
    /// </summary>
    public class PreguntasGuardar : TableEntity
    {
        public string Title { get; set; }
        public string RequesterName { get; set; }
        public string UserQuestion { get; set; }

        private string RowKeys;
        public string RowKeys_
        {
            get
            {
                return this.RowKeys;
            }
            set
            {
                this.RowKey = value;
                RowKeys_ = value;
            }
        }

        private String PartitionKeys;
        public String PartitionKey_
        {
            get
            {
                return this.PartitionKeys;
            }

            set
            {
                this.PartitionKey_ = value;
                this.PartitionKey = value;
            }
        }

    }
}
