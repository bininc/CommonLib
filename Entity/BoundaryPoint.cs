using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CommonLib.Entity
{
    /// <summary>
    /// 界址点 由X、Y、Z组成
    /// </summary>
    public class BoundaryPoint
    {
        [Display(Name = "X")]
        public string X { get; set; }
        [Display(Name = "Y")]
        public string Y { get; set; }
        [Display(Name = "Z")]
        public string Z { get; set; }
    }
}
