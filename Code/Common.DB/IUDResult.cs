using System.Collections;
namespace Common.DB
{
    /// <summary>
    /// 插入、修改、删除 结果
    /// </summary>
    public class IUDResult : NVCollection
    {
        public IUDResult()
        {

        }

        public IUDResult(IDictionary data)
        {
            this.Init(data);
        }

        public void Init(IDictionary data)
        {
            foreach (string k in data.Keys)
            {
                base.Add(k, data[k]);
            }
        }

        bool succ = false;
        public bool Success
        {
            get { return succ; }
            set { succ = value; }
        }

    }

}