using System;
using System.ComponentModel.DataAnnotations;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    public class ApiScriptMigration:Entity
    {
        [MaxLength(20)]
        public string VersionId { get; set; }
        [MaxLength(5000)]
        public string ScriptFolderName { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime? LastExecution { get; set; }
        public bool Execute { get; set; } = false;
        public int? TFSChangeSet { get; set; }
        public string FailedScriptLog { get; set; } = "";
    }
}
