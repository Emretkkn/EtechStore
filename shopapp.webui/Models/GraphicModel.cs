using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class GraphicModel
    {
       public List<int> Datas { get; set; }
       public List<string> Labels { get; set; }
       public List<Category> Category { get; set; }
    }
}