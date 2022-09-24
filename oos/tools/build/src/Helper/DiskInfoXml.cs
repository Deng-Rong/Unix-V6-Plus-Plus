using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Build
{
    public static class DiskInfo
    {
        public const string CYLINDERS = "Cylinders";
        public const string HEADS = "Heads";
        public const string SPT = "SPT";
        public const string BOOTANDKERNELSIZE = "BootAndKernelSize";
        public const string SUPERBLOCKSIZE = "SuperBlockSize";
        public const string INODESIZE = "InodeSize";
        public const string SWAPSIZE = "SwapSize";

        public static Dictionary<string, int> GetDiskInfo()
        {
            Dictionary<string, int> diskInfoHash = new Dictionary<string, int>();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Helper.DiskInfoConfigFileName);
            XmlElement xmlRoot = xmlDocument.DocumentElement;
            foreach (XmlNode xmlNode in xmlRoot)
            {
                int value = Convert.ToInt32(xmlNode.InnerText);
                diskInfoHash.Add(xmlNode.Name, value);
            }
            return diskInfoHash;
        }
    }
}
