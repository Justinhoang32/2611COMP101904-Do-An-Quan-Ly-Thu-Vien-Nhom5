using System.Drawing.Printing;
using QuanLyThuVien.Demo;

namespace QuanLyThuVien.GiaoDien;

internal static class PrintSupport
{
    public static void PrintText(string title,string text)
    {
        try
        {
            using var doc=new PrintDocument {DocumentName=title};int offset=0;
            doc.BeginPrint+=(_,_)=>offset=0;
            doc.PrintPage+=(_,e)=>
            {
                using var heading=new Font("Georgia",16,FontStyle.Bold);using var body=new Font("Segoe UI",10);
                e.Graphics!.DrawString(title,heading,Brushes.Black,e.MarginBounds.Left,e.MarginBounds.Top);
                var bounds=new RectangleF(e.MarginBounds.Left,e.MarginBounds.Top+45,e.MarginBounds.Width,e.MarginBounds.Height-45);
                using var format=new StringFormat(StringFormatFlags.LineLimit);
                e.Graphics.MeasureString(text[offset..],body,bounds.Size,format,out int fitted,out int linesFilled);
                if(fitted==0)throw new InvalidOperationException("Vùng in quá nhỏ.");
                e.Graphics.DrawString(text.Substring(offset,fitted),body,Brushes.Black,bounds,format);offset+=fitted;e.HasMorePages=offset<text.Length;
            };
            using var preview=new PrintPreviewDialog {Document=doc,Width=1000,Height=800};preview.ShowDialog();
        }
        catch(Exception ex) when(ex is InvalidPrinterException or InvalidOperationException or System.ComponentModel.Win32Exception) {Ui.Error("Không thể mở bản in: "+ex.Message);}
    }
}

internal static class ReceiptRenderer
{
    public static int RowsPerPage(bool landscape)=>landscape?1:6;
    public static int PageCount(DemoLoan loan,bool landscape)=>Math.Max(1,(loan.Lines.Count+RowsPerPage(landscape)-1)/RowsPerPage(landscape));

    public static void Draw(Graphics g,RectangleF target,DemoLoan loan,bool notes,bool landscape,int page)
    {
        float w=landscape?1123:794,h=landscape?794:1123;
        var state=g.Save();float scale=Math.Min(target.Width/w,target.Height/h);g.TranslateTransform(target.X,target.Y);g.ScaleTransform(scale,scale);
        g.FillRectangle(Brushes.White,0,0,w,h);
        using var normal=new Font("Times New Roman",12,GraphicsUnit.Pixel);
        using var bold=new Font("Times New Roman",12,FontStyle.Bold,GraphicsUnit.Pixel);
        using var title=new Font("Times New Roman",21,FontStyle.Bold,GraphicsUnit.Pixel);
        using var small=new Font("Times New Roman",10,GraphicsUnit.Pixel);
        using var center=new StringFormat {Alignment=StringAlignment.Center};using var wrap=new StringFormat {Trimming=StringTrimming.EllipsisCharacter};
        float left=50,right=w-50,width=w-100;
        void Text(string text,Font f,float x,float y,float width,float height=30,StringFormat? format=null)=>g.DrawString(text,f,Brushes.Black,new RectangleF(x,y,width,height),format??wrap);
        var logo=Ui.Asset("hcmue-logo.png");if(logo!=null){float lw=100,lh=logo.Height*lw/logo.Width;g.DrawImage(logo,left,45,lw,lh);}
        Text("TRƯỜNG ĐẠI HỌC SƯ PHẠM\nTHÀNH PHỐ HỒ CHÍ MINH",bold,w/2,44,w/2-50,42,center);
        Text("THƯ VIỆN",normal,w/2,86,w/2-50,24,center);g.DrawLine(Pens.Gray,left,124,right,124);
        Text("PHIẾU MƯỢN SÁCH",title,left,145,width,32,center);
        Text("Số: "+loan.Code,normal,left,185,width,25,center);
        Text($"Ngày lập: {loan.Borrowed:dd} tháng {loan.Borrowed:MM} năm {loan.Borrowed:yyyy}",normal,left,212,width,25,center);
        Text("THÔNG TIN ĐỘC GIẢ",bold,left,255,width);
        Text("Họ và tên: "+loan.Reader.Name,normal,left,282,width*.7F);Text("Mã thẻ: "+loan.Reader.Code,normal,right-155,282,155);
        Text($"Nhóm độc giả: {loan.Reader.Group} · Khoa: {loan.Reader.Faculty}",normal,left,308,width);
        Text($"Mã sinh viên: {loan.Reader.StudentId} · Điện thoại: {loan.Reader.Phone}",normal,left,334,width);
        Text($"Ngày mượn: {loan.Borrowed:dd/MM/yyyy}",normal,left,360,width*.6F);Text($"Ngày hẹn trả: {loan.Due:dd/MM/yyyy}",bold,right-205,360,205);
        float y=398;float[] fractions=[.06F,.10F,.40F,.18F,.07F,.19F];string[] headers=["STT","Mã sách","Tên sách / Tác giả","Mã bản","SL","Tình trạng"];
        using var fill=new SolidBrush(Color.FromArgb(240,240,240));g.FillRectangle(fill,left,y,width,32);
        void Row(string[] values,float height,bool heading)
        {
            float x=left;for(int i=0;i<values.Length;i++){float cw=fractions[i]*width;g.DrawRectangle(Pens.Silver,x,y,cw,height);Text(values[i],heading?bold:normal,x+7,y+10,cw-14,height-12);x+=cw;}y+=height;
        }
        Row(headers,32,true);int rows=RowsPerPage(landscape);var lines=loan.Lines.Skip(page*rows).Take(rows).ToList();
        for(int i=0;i<lines.Count;i++){var l=lines[i];Row([(page*rows+i+1).ToString(),l.Book.Code,l.Book.Title+"\n"+l.Book.Author,l.CopyCode,"1","Tốt"],landscape?43:56,false);}
        g.DrawRectangle(Pens.Gray,left,y,width,32);Text($"Tổng cộng: {loan.Lines.Select(x=>x.Book.Code).Distinct().Count()} đầu sách",bold,left+8,y+9,width-150);Text(loan.Lines.Count+" cuốn",bold,right-90,y+9,80);y+=50;
        if(notes&&!string.IsNullOrWhiteSpace(loan.Note)){Text("Ghi chú: "+loan.Note,normal,left,y,width,45);y+=50;}
        if(page==PageCount(loan,landscape)-1)
        {
            Text("Độc giả đã nhận đủ sách, kiểm tra tình trạng và cam kết giữ gìn sách, trả đúng hạn. Khi sách hư hỏng hoặc mất, vui lòng liên hệ thư viện để được hướng dẫn xử lý.",normal,left,y,width,45);y+=48;
            Text($"Thành phố Hồ Chí Minh, ngày {loan.Borrowed:dd} tháng {loan.Borrowed:MM} năm {loan.Borrowed:yyyy}",normal,left,y,width,30,center);y+=35;
            Text("NGƯỜI MƯỢN",bold,left,y,width/2,25,center);Text("THỦ THƯ",bold,left+width/2,y,width/2,25,center);
            Text("(Ký và ghi rõ họ tên)",small,left,y+24,width/2,25,center);Text("(Ký và ghi rõ họ tên)",small,left+width/2,y+24,width/2,25,center);
            Text(loan.Reader.Name,normal,left,Math.Min(y+90,h-75),width/2,25,center);Text(loan.Staff,normal,left+width/2,Math.Min(y+90,h-75),width/2,25,center);
        }
        g.DrawLine(Pens.LightGray,left,h-48,right,h-48);Text("Thư viện HCMUE · Dữ liệu minh họa",small,left,h-34,width-100);Text($"Trang {page+1}/{PageCount(loan,landscape)}",small,right-70,h-34,70);
        g.Restore(state);
    }
}

internal sealed class ReceiptCanvas : Control
{
    public DemoLoan Loan {get;set;}=new();
    public bool IncludeNotes {get;set;}=true;
    public bool Landscape {get;set;}
    public int Page {get;set;}
    public ReceiptCanvas(){DoubleBuffered=true;BackColor=Color.White;Dock=DockStyle.Top;}
    protected override void OnResize(EventArgs e){base.OnResize(e);int desired=(int)(Width*(Landscape?794F/1123:1123F/794));if(Height!=desired)Height=desired;Invalidate();}
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);ReceiptRenderer.Draw(e.Graphics,ClientRectangle,Loan,IncludeNotes,Landscape,Page);}
}
