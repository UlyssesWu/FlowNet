using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    /// <summary>
    /// 动作列表
    /// </summary>
    public class ActionList : Dictionary<OfpActionType, IOfpAction>,IToByteArray
    {
        /// <summary>
        /// 检查动作是否合法
        /// </summary>
        /// <returns></returns>
        public bool CheckActions()
        {
            //TODO: do we need Enum.IsDefined check?
            foreach (var ofpAction in this)
            {
                if (ofpAction.Value.Header.Type != ofpAction.Key) return false;
            }
            return true;
        }

        internal bool CheckActions(out string info)
        {
            foreach (var ofpAction in this)
            {
                if (ofpAction.Value.Header.Type != ofpAction.Key)
                {
                    info = $"{ofpAction.Value.GetType()} type can not match {ofpAction.Key} OfpActionType.";
                    return false;
                }
            }
            info = "";
            return true;
        }

        /// <summary>
        /// 动作列表的字节长度
        /// </summary>
        public int Length
        {
            //TODO: length check
            get
            {
                return Values.Aggregate(0, (current, action) => current + action.Header.Len);
            }
        }

        public byte[] ToByteArray()
        {
            string info;
            if (!CheckActions(out info))
            {
                throw new FormatException("Actions not match!\n" + info);
            }
            Writer w = new Writer();
            foreach (var action in Values)
            {
                w.Write(action.ToByteArray());
            }
            return w.ToByteArray();
        }
    }
}
