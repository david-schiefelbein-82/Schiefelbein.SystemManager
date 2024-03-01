using System;
using System.Text.Json.Serialization;

namespace Schiefelbein.SystemManager.Models
{
    /// <summary>
    /// </summary>
    public class FolderViewModel
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public List<FolderViewModel> Children { get; set; }

        public bool? HasChildren { get; set; }

        public bool? Opened { get; set; }

        public bool? Selected { get; set; }

        public FolderViewModel(string id, string text)
        {
            Id = id;
            Text = text;
            Children = new List<FolderViewModel>();
        }
    }
}
