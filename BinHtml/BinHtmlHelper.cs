using CommonLib.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CommLiby;

namespace CommonLib.BinHtml
{
    /// <summary>
    /// HMTL帮助类
    /// </summary>
    public static class BinHtmlHelper
    {
        /// <summary>
        /// 图片查看器代码
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static HtmlString ImageViewerHTML(string title, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new HtmlString("<a href=\"#\">无</a>");
            }
            return
                new HtmlString(
                    string.Format("<a href=\"#\" onclick=\"openImgWin('{0}', '{1}');return false;\">查看</a>",
                        title,
                        path));
        }

        /// <summary>
        /// 获得图片控件代码
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="title">标题</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static HtmlString ImageHTML(ModelType type, string name, string title, string path)
        {
            if (type == ModelType.Details)
            {
                return ImageViewerHTML(title, path);
            }
            if (type == ModelType.Create)
            {
                return new HtmlString(string.Format("<input type=\"file\" name=\"{0}\" file=\"image\" value=\"{1}\"/>", name, path));
            }
            if (type == ModelType.Edit)
            {
                return new HtmlString(ImageViewerHTML(title, path) + string.Format("-><input type=\"file\" name=\"{0}\" style=\"max-width:170px;\" file=\"image\"/>", name));
            }
            return null;
        }

        /// <summary>
        /// 视频查看器代码
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static HtmlString VideoViewerHTML(string title, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new HtmlString("<a href=\"#\">无</a>");
            }
            return
                new HtmlString(
                    string.Format("<a href=\"#\" onclick=\"openImgWin('{0}', '{1}');return false;\">播放</a>",
                        title,
                        path));
        }

        /// <summary>
        /// 获得视频控件代码
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="title">标题</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static HtmlString VideoHTML(ModelType type, string name, string title, string path)
        {
            if (type == ModelType.Details)
            {
                return ImageViewerHTML(title, path);
            }
            if (type == ModelType.Create)
            {
                return new HtmlString(string.Format("<input type=\"file\" name=\"{0}\" file=\"video\"/>", name));
            }
            if (type == ModelType.Edit)
            {
                return new HtmlString(VideoViewerHTML(title, path) + string.Format("-><input type=\"file\" name=\"{0}\" style=\"width:170px;\" file=\"video\"/>", name));
            }
            return null;
        }

        /// <summary>
        /// 获得文件控件代码
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="name">提交名字</param>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static HtmlString FileHTML(ModelType type, string name, string path)
        {
            if (type == ModelType.Details)
            {
                return FileViewerHTML(path);
            }
            if (type == ModelType.Create)
            {
                return new HtmlString(string.Format("<input type=\"file\" name=\"{0}\" file=\"file\"/>", name));
            }
            if (type == ModelType.Edit)
            {
                return new HtmlString(FileViewerHTML(path) + string.Format("-><input type=\"file\" name=\"{0}\" style=\"width:170px;\" file=\"file\"/>", name));
            }
            return null;
        }

        /// <summary>
        /// 获得文件控件查看代码
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static HtmlString FileViewerHTML(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new HtmlString("<a href=\"#\">无</a>");
            }
            return new HtmlString(string.Format("<a href=\"#\" onclick=\"openImgWin('', '{0}');return false;\">下载文件</a>", path));
        }

        /// <summary>
        /// 时间选择控件HTML
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HtmlString DateTimeHTML(string name, BinDateTime value)
        {
            return new HtmlString(string.Format("<input type=\"text\" class=\"datechoose\" name=\"{0}.DateTime\" value=\"{1}\" />", name, value));
        }
    }
}