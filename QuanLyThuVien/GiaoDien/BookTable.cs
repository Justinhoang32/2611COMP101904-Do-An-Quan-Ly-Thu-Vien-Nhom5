using QuanLyThuVien.Demo;

namespace QuanLyThuVien.GiaoDien;

internal static class BookTable
{
    public static DataGridView Create(bool search)
    {
        var grid=Ui.Grid(search?445:470);
        grid.Columns.Add(new DataGridViewImageColumn {Name="cover",HeaderText="",Width=44,AutoSizeMode=DataGridViewAutoSizeColumnMode.None,ImageLayout=DataGridViewImageCellLayout.Zoom});
        Ui.Column(grid,"book","SÁCH / MÃ",210);Ui.Column(grid,"author","TÁC GIẢ",130);Ui.Column(grid,"category","THỂ LOẠI",100);
        if(search) Ui.Column(grid,"location","VỊ TRÍ",80);
        Ui.Column(grid,"copies",search?"CÒN / TỔNG":"TỔNG / CÒN",80);
        if(!search) Ui.Column(grid,"status","TRẠNG THÁI",90);
        Ui.Column(grid,"action","THAO TÁC",95,true);
        if(search) Ui.Column(grid,"add","",105,true);
        grid.RowTemplate.Height=76;
        grid.Tag=new List<Image>();
        grid.Disposed+=(_,_)=>DisposeCovers(grid);
        return grid;
    }
    public static void Clear(DataGridView grid)
    {
        DisposeCovers(grid);
        grid.Rows.Clear();
    }
    private static void DisposeCovers(DataGridView grid)
    {
        if(grid.Tag is not List<Image> images)return;
        foreach(var image in images)image.Dispose();
        images.Clear();
    }
    public static void Fill(DataGridView grid,IEnumerable<DemoBook> books,bool search)
    {
        Clear(grid);
        foreach(var b in books)
        {
            var cover=Cover(b);((List<Image>)grid.Tag!).Add(cover);
            int i=search?grid.Rows.Add(cover,b.Title+"\n"+b.Code+" · Tiếng Việt · "+b.Year,b.Author,b.Category,b.Location,$"{b.Available} / {b.Total}","Chi tiết sách",b.Borrowable&&b.Available>0?"+ Thêm vào phiếu":""):
                grid.Rows.Add(cover,b.Title+"\n"+b.Code+" · NXB Đại học Sư phạm",b.Author,b.Category,$"{b.Total} / {b.Available}",b.Available>0?"Có sẵn":"Hết sách","Sửa");
            grid.Rows[i].Tag=b;grid.Rows[i].Cells["book"].Style.Font=Ui.BoldFont;
            if(search) grid.Rows[i].Cells["copies"].Style.ForeColor=b.Available>0?Ui.Green:Ui.Red;
            else Ui.Tone(grid.Rows[i].Cells["status"],b.Available>0?"Có sẵn":"Hết sách");
        }
    }
    public static Bitmap Cover(DemoBook book,int width=34,int height=46)
    {
        if (book.CoverImage != null)
        {
            using var stream = new MemoryStream(book.CoverImage);
            using var original = Image.FromStream(stream);
            return new Bitmap(original, width, height);
        }
        Color[] colors=[Ui.Navy,Color.FromArgb(98,126,112),Color.FromArgb(149,113,96),Color.FromArgb(104,109,140),Color.FromArgb(128,127,103),Color.FromArgb(64,110,124)];
        var image=new Bitmap(width,height);using var g=Graphics.FromImage(image);
        int index=int.TryParse(book.Code.TrimStart('S'),out int id)?id-1:0;g.Clear(colors[Math.Abs(index)%colors.Length]);
        using var white=new SolidBrush(Color.White);using var rule=new Pen(Color.FromArgb(130,Color.White));
        using var font=new Font("Georgia",Math.Max(4,width/7F),GraphicsUnit.Pixel);
        g.DrawLine(rule,4,5,width-6,5);g.DrawString(book.Title,font,white,new RectangleF(5,8,width-10,height-13));return image;
    }
}
