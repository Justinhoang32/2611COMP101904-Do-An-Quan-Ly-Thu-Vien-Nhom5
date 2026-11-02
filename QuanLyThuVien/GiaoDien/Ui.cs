using System.Drawing.Drawing2D;
using System.Reflection;

namespace QuanLyThuVien.GiaoDien;

internal static class Ui
{
    public static readonly Color Navy = Color.FromArgb(21, 72, 117);
    public static readonly Color Text = Color.FromArgb(37, 61, 83);
    public static readonly Color Muted = Color.FromArgb(123, 142, 160);
    public static readonly Color Background = Color.FromArgb(243, 245, 247);
    public static readonly Color Border = Color.FromArgb(218, 228, 236);
    public static readonly Color Blue = Color.FromArgb(232, 241, 247);
    public static readonly Color Green = Color.FromArgb(35, 120, 93);
    public static readonly Color Red = Color.FromArgb(222, 53, 66);
    public static readonly Color Amber = Color.FromArgb(164, 105, 23);
    public static readonly Font BodyFont = new("Segoe UI", 10F);
    public static readonly Font SmallFont = new("Segoe UI", 8.5F);
    public static readonly Font BoldFont = new("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Dictionary<string, Image> images = [];

    public static Image? Asset(string name)
    {
        if (images.TryGetValue(name, out var image)) return image;
        var assembly = Assembly.GetExecutingAssembly();
        string? resource = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + name, StringComparison.OrdinalIgnoreCase));
        if (resource == null) return null;
        using var stream = assembly.GetManifestResourceStream(resource);
        using var original = Image.FromStream(stream!);
        return images[name] = new Bitmap(original);
    }

    public static Label Label(string text, bool bold = false, Color? color = null, float? size = null)
        => new() { Text = text, AutoSize = true, Font = size.HasValue ? new Font("Segoe UI", size.Value, bold ? FontStyle.Bold : FontStyle.Regular) : bold ? BoldFont : BodyFont,
            ForeColor = color ?? Text, Margin = new Padding(0, 0, 0, 8), BackColor = Color.Transparent };

    public static Label Heading(string text, float size = 23)
        => new() { Text = text, AutoSize = true, Font = new Font("Georgia", size, FontStyle.Bold), ForeColor = Navy, Margin = new Padding(0, 0, 0, 10), BackColor = Color.Transparent };

    public static Button Button(string text, Action action, bool primary = false, string? icon = null)
    {
        var b = new Button { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(40, 40), Padding = new Padding(12, 4, 12, 4), Margin = new Padding(0, 0, 10, 0),
            FlatStyle = FlatStyle.Flat, BackColor = primary ? Navy : Color.White, ForeColor = primary ? Color.White : Navy,
            Font = BoldFont, Cursor = Cursors.Hand, TextImageRelation = TextImageRelation.ImageBeforeText, ImageAlign = ContentAlignment.MiddleLeft,
            Image = icon == null ? null : Asset($"{icon}{(primary ? "-white" : "")}.png") };
        b.FlatAppearance.BorderColor = primary ? Navy : Border;
        b.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(16, 57, 95) : Blue;
        if (icon != null)
        {
            var tip = new ToolTip();
            b.AccessibleName = text.Length > 0 ? text : icon;
            b.MouseEnter += (_, _) => tip.SetToolTip(b, b.AccessibleName);
            b.Disposed += (_, _) => tip.Dispose();
        }
        b.Click += (_, _) => action(); return b;
    }

    public static TextBox Input(string placeholder = "") => new() { PlaceholderText = placeholder, Font = BodyFont, BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Fill, Margin = new Padding(0), MinimumSize = new Size(0, 34) };

    public static ComboBox Select(params string[] values)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = BodyFont, Dock = DockStyle.Fill, Margin = Padding.Empty, IntegralHeight = false, DropDownHeight = 240 };
        c.Items.AddRange(values); if (values.Length > 0) c.SelectedIndex = 0; return c;
    }

    public static DateTimePicker Date(DateTime value) => new() { Value = value, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Dock = DockStyle.Fill, Font = BodyFont, MinimumSize = new Size(0, 32) };
    public static NumericUpDown Number(int value, int min = 0, int max = 100000)
    {
        var num = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = (value >= min && value <= max) ? value : min,
            Font = BodyFont,
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 32)
        };
        return num;
    }

    public static TableLayoutPanel Stack(params Control[] items)
    {
        var p = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 0,
            Dock = DockStyle.Top, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent };
        p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var item in items) { int row = p.RowCount++; p.RowStyles.Add(new RowStyle(SizeType.AutoSize)); p.Controls.Add(item, 0, row); item.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; }
        return p;
    }

    public static TableLayoutPanel Columns(float[] widths, params Control[] items)
    {
        var p = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = widths.Length, RowCount = 1, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 18) };
        foreach (float width in widths) p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
        for (int i = 0; i < items.Length; i++) { p.Controls.Add(items[i], i, 0); items[i].Dock = DockStyle.Top; items[i].Margin = new Padding(0, 0, i < items.Length-1 ? 18 : 0, 0); }
        return p;
    }

    public static Control Field(string title, Control input) => Stack(Label(title, true, size: 9), input);
    public static FlowLayoutPanel Actions(params Control[] controls)
    {
        var p = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
            Dock = DockStyle.Top, Margin = new Padding(0, 8, 0, 8), Padding = Padding.Empty };
        p.Controls.AddRange(controls); return p;
    }

    public static Surface Card(string title, string subtitle, params Control[] content)
    {
        var p = new Surface { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 22), Dock = DockStyle.Top };
        List<Control> items = [];
        if (title.Length != 0) items.Add(Heading(title, 13.5F));
        if (subtitle.Length != 0) items.Add(Label(subtitle, color: Muted, size: 9));
        items.AddRange(content); p.Controls.Add(Stack(items.ToArray())); return p;
    }

    public static Control Notice(string message, bool error = false)
    {
        var text = Label(message, color: error ? Red : Navy, size: 9);
        text.Dock = DockStyle.Fill; text.Margin = Padding.Empty;
        var p = new Surface { AutoSize = true, Padding = new Padding(14), BackColor = error ? Color.FromArgb(253,238,240) : Blue, Margin = new Padding(0, 0, 0, 18) };
        p.Controls.Add(text); p.SizeChanged += (_, _) => text.MaximumSize = new Size(Math.Max(100, p.ClientSize.Width-28), 0); return p;
    }

    public static Control Metric(string label, string number, string detail, bool red = false)
        => Card("", "", Label(label, color: Muted, size: 9), new Label { Text = number, AutoSize = true, Font = new Font("Georgia", 24), ForeColor = red ? Red : Navy, Margin = new Padding(0, 8, 0, 14) }, Label(detail, color: red ? Red : Muted, size: 8.5F));

    public static DataGridView Grid(int height = 430)
    {
        var g = new DataGridView { Height = height, Dock = DockStyle.Top, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, ReadOnly = true,
            RowHeadersVisible = false, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, EnableHeadersVisualStyles = false,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            GridColor = Border, ColumnHeadersHeight = 40, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            Margin = new Padding(0, 10, 0, 6), Font = BodyFont };
        g.DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Text, BackColor = Color.White, SelectionBackColor = Blue, SelectionForeColor = Navy,
            Padding = new Padding(8), WrapMode = DataGridViewTriState.True };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(246,248,249), ForeColor = Muted, Font = SmallFont, Padding = new Padding(8) };
        g.RowTemplate.Height = 66; return g;
    }

    public static void Column(DataGridView g, string name, string header, float width = 100, bool link = false)
    {
        DataGridViewColumn c = link ? new DataGridViewLinkColumn { LinkColor = Navy, ActiveLinkColor = Navy, VisitedLinkColor = Navy, TrackVisitedState = false } : new DataGridViewTextBoxColumn();
        c.Name = name; c.HeaderText = header; c.FillWeight = width; c.SortMode = DataGridViewColumnSortMode.NotSortable; c.MinimumWidth = (int)Math.Min(70,width); g.Columns.Add(c);
    }

    public static void Tone(DataGridViewCell cell, string text)
    {
        cell.Style.ForeColor = text.Contains("Trễ") || text.Contains("quá hạn") || text == "Hết sách" ? Red : text is "Đã trả" or "Có sẵn" or "Hoạt động" ? Green : text.Contains("phần") || text.Contains("hết hạn") ? Amber : Navy;
        cell.Style.Font = BoldFont;
    }

    public static void Info(string message) => MessageBox.Show(message, "Thư viện HCMUE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static void Export(string filename,string[] headers,IEnumerable<string[]> rows)
    {
        using var dialog=new SaveFileDialog {Filter="Excel workbook (*.xlsx)|*.xlsx",FileName=filename,DefaultExt="xlsx",AddExtension=true};
        if(dialog.ShowDialog()!=DialogResult.OK)return;
        Try(()=>{Demo.ExcelExport.Write(dialog.FileName,headers,rows);Info("Đã xuất danh sách.");});
    }
    public static string[]? Ask(string title,params (string Label,string Value)[] fields)
    {
        using var form=new Form {Text=title,StartPosition=FormStartPosition.CenterParent,Width=500,Height=150+fields.Length*75,Font=BodyFont,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,Padding=new Padding(24)};
        var inputs=fields.Select(f=>Input()).ToArray();
        var items=new List<Control>();
        for(int i=0;i<fields.Length;i++){inputs[i].Text=fields[i].Value;var field=Field(fields[i].Label,inputs[i]);field.Margin=new Padding(0,0,0,16);items.Add(field);}
        var save=Button("Lưu",()=>form.DialogResult=DialogResult.OK,true,"check");var cancel=Button("Hủy",()=>form.DialogResult=DialogResult.Cancel);
        items.Add(Actions(cancel,save));form.Controls.Add(Stack(items.ToArray()));form.AcceptButton=save;form.CancelButton=cancel;
        return form.ShowDialog()==DialogResult.OK?inputs.Select(x=>x.Text.Trim()).ToArray():null;
    }
    public static void Error(string message) => MessageBox.Show(message, "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Try(Action action) { try { action(); } catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException or ArgumentException or Microsoft.VisualBasic.FileIO.MalformedLineException) { Error(ex.Message); } }
}

internal sealed class Surface : Panel
{
    public Surface() { DoubleBuffered = true; BackColor = Color.White; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Ui.Border);
        var r = new Rectangle(0, 0, Width-1, Height-1);
        if (r.Width < 16 || r.Height < 16) return;
        using var path = new GraphicsPath(); int d = 12;
        path.AddArc(r.Left,r.Top,d,d,180,90); path.AddArc(r.Right-d,r.Top,d,d,270,90);
        path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90); path.AddArc(r.Left,r.Bottom-d,d,d,90,90); path.CloseFigure();
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class BarChart : Control
{
    public string[] Labels { get; set; } = [];
    public int[] Values { get; set; } = [];
    public BarChart() { DoubleBuffered = true; Height = 240; Dock = DockStyle.Top; BackColor = Color.White; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); if (Values.Length == 0) return;
        int max = Math.Max(1, Values.Max()); float slot = (Width-32F)/Values.Length;
        using var light = new SolidBrush(Color.FromArgb(158,186,205)); using var navy = new SolidBrush(Ui.Navy); using var muted = new SolidBrush(Ui.Muted);
        using var format = new StringFormat { Alignment = StringAlignment.Center };
        for (int i=0;i<Values.Length;i++)
        {
            float h = (Height-62F)*Values[i]/max, x=16+i*slot+slot*.22F;
            e.Graphics.FillRectangle(i==Values.Length-1?navy:light, x, Height-30-h, slot*.56F, h);
            e.Graphics.DrawString(Values[i].ToString(),Ui.SmallFont,muted,new RectangleF(x-slot*.15F,Height-52-h,slot*.86F,20),format);
            e.Graphics.DrawString(Labels[i],Ui.SmallFont,muted,new RectangleF(x-slot*.15F,Height-24,slot*.86F,20),format);
        }
    }
}
