using System.Windows.Controls;
using System.Windows.Documents;

namespace TocTinyWPF.ViewModel
{
    internal class MsgViewModel
    {
        /// <summary>
        /// 发言者
        /// </summary>
        public string Publisher { get; set; } = "Null";
        /// <summary>
        /// 内容
        /// </summary>
        public TextBlock Content { get; set; } = new TextBlock(new Run("我要撒了你"));
    }
}
