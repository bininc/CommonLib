using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.Entity
{
    public enum ModelType
    {
        /// <summary>
        /// 浏览
        /// </summary>
        Details,
        /// <summary>
        /// 编辑
        /// </summary>
        Edit,
        /// <summary>
        /// 创建
        /// </summary>
        Create
    }

    public class BaseModel
    {
        private ModelType _type;

        /// <summary>
        /// 模型类型 （默认浏览模式）
        /// </summary>
        public ModelType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public BaseModel()
        {
            _titleName = SetTitleName();
            Type = ModelType.Details;
        }

        private string _titleName;

        public string TitleName
        {
            get
            {
                switch (Type)
                {
                    case ModelType.Create:
                        return( "添加" + _titleName);
                    case ModelType.Details:
                        return (_titleName + "详细信息");
                    case ModelType.Edit:
                        return ("修改" + _titleName);
                }
                return _titleName;
            }
            set { _titleName = value; }
        }

        protected virtual string SetTitleName()
        {
            return "未设置标题";
        }
    }
}
