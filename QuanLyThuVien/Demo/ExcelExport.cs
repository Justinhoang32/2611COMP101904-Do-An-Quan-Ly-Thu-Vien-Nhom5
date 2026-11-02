using System.IO.Compression;
using System.Xml.Linq;

namespace QuanLyThuVien.Demo;

// Minimal OOXML workbook. Inline strings prevent spreadsheet formula injection.
public static class ExcelExport
{
    public static void Write(string path,string[] headers,IEnumerable<string[]> rows)
    {
        XNamespace s="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace r="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace rel="http://schemas.openxmlformats.org/package/2006/relationships";
        XNamespace ct="http://schemas.openxmlformats.org/package/2006/content-types";
        using var file=File.Create(path);using var zip=new ZipArchive(file,ZipArchiveMode.Create);
        void Add(string name,XElement root)
        {
            using var stream=zip.CreateEntry(name).Open();new XDocument(new XDeclaration("1.0","utf-8",null),root).Save(stream);
        }
        Add("[Content_Types].xml",new XElement(ct+"Types",
            new XElement(ct+"Default",new XAttribute("Extension","rels"),new XAttribute("ContentType","application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(ct+"Default",new XAttribute("Extension","xml"),new XAttribute("ContentType","application/xml")),
            new XElement(ct+"Override",new XAttribute("PartName","/xl/workbook.xml"),new XAttribute("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(ct+"Override",new XAttribute("PartName","/xl/worksheets/sheet1.xml"),new XAttribute("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))));
        Add("_rels/.rels",new XElement(rel+"Relationships",new XElement(rel+"Relationship",new XAttribute("Id","rId1"),new XAttribute("Type",r.NamespaceName+"/officeDocument"),new XAttribute("Target","xl/workbook.xml"))));
        Add("xl/workbook.xml",new XElement(s+"workbook",new XAttribute(XNamespace.Xmlns+"r",r),new XElement(s+"sheets",new XElement(s+"sheet",new XAttribute("name","Danh sách"),new XAttribute("sheetId",1),new XAttribute(r+"id","rId1")))));
        Add("xl/_rels/workbook.xml.rels",new XElement(rel+"Relationships",new XElement(rel+"Relationship",new XAttribute("Id","rId1"),new XAttribute("Type",r.NamespaceName+"/worksheet"),new XAttribute("Target","worksheets/sheet1.xml"))));
        var data=new XElement(s+"sheetData");int number=0;
        foreach(var values in new[]{headers}.Concat(rows))
        {
            var row=new XElement(s+"row",new XAttribute("r",++number));
            for(int i=0;i<values.Length;i++) row.Add(new XElement(s+"c",new XAttribute("r",ColumnName(i+1)+number),new XAttribute("t","inlineStr"),new XElement(s+"is",new XElement(s+"t",new XAttribute(XNamespace.Xml+"space","preserve"),values[i]??""))));
            data.Add(row);
        }
        Add("xl/worksheets/sheet1.xml",new XElement(s+"worksheet",new XElement(s+"cols",Enumerable.Range(1,headers.Length).Select(i=>new XElement(s+"col",new XAttribute("min",i),new XAttribute("max",i),new XAttribute("width",25),new XAttribute("customWidth",1)))),data));
    }
    private static string ColumnName(int n) { string name="";while(n>0){n--;name=(char)('A'+n%26)+name;n/=26;}return name; }
}
