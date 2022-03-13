using System.Collections.Generic;
using System.Xml;

namespace LiveBot
{
    class XmlUtility
    {
        public XmlUtility() { xmlDocument = new XmlDocument(); }
        public string Path { get; set; }

        private XmlDocument xmlDocument;

        public void Load()
        {
            try
            {
                if (Path == null)
                    return;
                xmlDocument.Load(Path);
            }
            catch
            {
                new LiveToast().ToastText("读取XML文件失败" + Path);
            }

        }

        /// <summary>
        /// 向父节点加入子节点
        /// </summary>
        /// <param name="rootPath">父节点路径</param>
        /// <param name="element">子节点的元素对象</param>
        /// <param name="allowRepeat">是否允许子节点重复</param>
        /// <returns>添加结果</returns>
        public bool AppendChild(string rootPath, XmlElement element, bool allowRepeat)
        {
            XmlNode node = xmlDocument.SelectSingleNode(rootPath);

            if (!allowRepeat && FindRepeatAttr(rootPath, "roomid", element))
            {
                return false;
            }
            node.AppendChild(element);
            Save();
            return true;
        }

        /// <summary>
        /// 删除节点 需要完全一致
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="element">需要删除的节点的元素对象</param>
        /// <returns>删除结果</returns>
        public bool DeleteChild(string rootPath, XmlElement element)
        {
            try
            {
                var rootNode = xmlDocument.DocumentElement;
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    if (node.OuterXml == element.OuterXml)
                        rootNode.RemoveChild(node);
                }
                Save();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 获取所有目标节点属性
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="nodeName">目标节点名字</param>
        /// <param name="attr">目标属性</param>
        /// <returns>属性内容列表</returns>
        public List<string> GetAllChildAttr(string rootPath, string nodeName, string attr)
        {
            List<string> attrList = new List<string>();
            XmlNodeList list = xmlDocument.SelectNodes(rootPath + "/" + nodeName);
            foreach (XmlNode node in list)
            {
                attrList.Add(node.Attributes[attr].Value);
            }
            return attrList;
        }

        /// <summary>
        /// 获取目标节点属性
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="nodeName">目标节点名字</param>
        /// <param name="attr">目标属性</param>
        /// <returns></returns>
        public string GetChildAttr(string rootPath, string nodeName, string attr)
        {
            XmlNode node = xmlDocument.SelectSingleNode(rootPath + "/" + nodeName);
            return node.Attributes[attr] == null ? "" : node.Attributes[attr].Value;
        }

        /// <summary>
        /// 获取所有目标节点内容
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="nodeName">目标节点名字</param>
        /// <returns></returns>
        public List<string> GetAllChildText(string rootPath, string nodeName)
        {
            List<string> attrList = new List<string>();
            XmlNodeList list = xmlDocument.SelectNodes(rootPath + "/" + nodeName);
            foreach (XmlNode node in list)
            {
                attrList.Add(node.InnerText);
            }
            return attrList;
        }

        /// <summary>
        /// 获取节点内容文本
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="nodeName">目标节点名字</param>
        /// <returns>目标节点内容文本</returns>
        public string GetChildText(string rootPath, string nodeName)
        {
            XmlNode node = xmlDocument.SelectSingleNode(rootPath + "/" + nodeName);
            return node == null ? "" : node.InnerText;
        }

        /// <summary>
        /// 设置节点内容
        /// </summary>
        /// <param name="rootPath">目标节点的父节点路径</param>
        /// <param name="nodeName">目标节点名字</param>
        /// <param name="text">需要设置的文本</param>
        public void SetChildText(string rootPath, string nodeName, string text)
        {
            XmlNode node = xmlDocument.SelectSingleNode(rootPath + "/" + nodeName);
            if (node == null)
                return;
            node.InnerText = text;
            Save();
        }

        /// <summary>
        /// 创建xml元素
        /// </summary>
        /// <param name="nodeName">元素名字</param>
        /// <returns></returns>
        public XmlElement CreateElement(string nodeName)
        {
            return xmlDocument.CreateElement(nodeName);
        }

        public bool Save()
        {
            try
            {
                xmlDocument.Save(Path);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 查找重复的属性
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="attr"></param>
        /// <param name="element"></param>
        /// <returns>返回执行结果</returns>
        private bool FindRepeatAttr(string rootPath, string attr, XmlElement element)
        {
            XmlNodeList nodeList = xmlDocument.SelectNodes(rootPath + "/" + element.Name);
            foreach (XmlNode node in nodeList)
            {
                if (node.Attributes[attr].Value == element.Attributes[attr].Value)
                    return true;
            }
            return false;
        }


    }
}
