namespace Dragonfly.UmbracoSiteTester.Models
{
	using System;

	public class FileInfo
    {
        public string Folder { get; set; }
        public string Filename { get; set; }
        public string Extension { get; set; }
        public string Domain { get; set; }
        public DateTime Timestamp { get; set; }
        public int StartNode { get; set; }

    }
}
