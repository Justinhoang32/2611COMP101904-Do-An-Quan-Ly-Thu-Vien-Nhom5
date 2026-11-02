using QuanLyThuVien.Demo;

namespace QuanLyThuVien.GiaoDien;

public enum LibraryScreen { Overview, Books, Catalog, Readers, Loans, CreateLoan, Returns, Search, Overdue, Print }

public class LibraryPage : Form
{
    protected LibraryDemo Store { get; private set; } = new();
    protected FlowLayoutPanel Body { get; } = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
        AutoScroll = true, Padding = new Padding(30), BackColor = Ui.Background };
    public event Action<LibraryScreen, object?>? Navigate;
    protected void Go(LibraryScreen screen, object? value = null) => Navigate?.Invoke(screen, value);

    protected void Setup(LibraryDemo store, string title)
    {
        Store = store; Text = title; Font = Ui.BodyFont; BackColor = Ui.Background;
        FormBorderStyle = FormBorderStyle.None; TopLevel = false; Dock = DockStyle.Fill; Controls.Add(Body);
        Body.ClientSizeChanged += (_, _) => ResizeContent();
    }

    protected void Header(string title, string description, params Control[] actions)
    {
        var header = Ui.Columns([65,35], Ui.Stack(Ui.Heading(title), Ui.Label(description, color: Ui.Muted, size:9)), Ui.Actions(actions));
        Add(header);
    }

    protected void Add(Control control) { Body.Controls.Add(control); ResizeContent(); }
    protected void Footer()
        => Add(Ui.Label("Thư viện · Trường Đại học Sư phạm Thành phố Hồ Chí Minh                         Dữ liệu minh họa · 02/11/2026", color: Ui.Muted, size:8));

    private void ResizeContent()
    {
        int width = Math.Max(760, Body.ClientSize.Width-Body.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-2);
        foreach (Control c in Body.Controls)
        {
            c.MinimumSize = new Size(width, c.MinimumSize.Height);
            c.MaximumSize = new Size(width, 0);
            c.Width = width;
            if (c is Label l) l.MaximumSize = new Size(width, 0);
        }
    }
}

internal sealed class Pager
{
    private int current = 1;
    private int total;
    private readonly int size;
    private readonly Action<int> changed;
    private readonly Label count = Ui.Label("", color: Ui.Muted, size:9);
    private readonly Label page = Ui.Label("", true, Ui.Navy);
    private readonly Button previous;
    private readonly Button next;
    public Control Control { get; }
    public int Current => current;
    public int Skip => (current-1)*size;
    public Pager(int size, Action<int> changed)
    {
        this.size=size; this.changed=changed;
        previous=Ui.Button("",()=>Move(-1),icon:"chevron-left"); previous.AccessibleName="Trang trước";
        next=Ui.Button("",()=>Move(1),icon:"chevron-right"); next.AccessibleName="Trang sau";
        Control=Ui.Columns([65,35],count,Ui.Actions(previous,page,next));
    }
    public void Update(int total, bool reset=false)
    {
        this.total=total; if(reset) current=1; current=Math.Clamp(current,1,Math.Max(1,(total+size-1)/size));
        count.Text=$"Hiển thị {(total==0?0:Skip+1)}–{Math.Min(total,Skip+size)} / {total} kết quả";
        page.Text=$"{current} / {Math.Max(1,(total+size-1)/size)}"; previous.Enabled=current>1; next.Enabled=Skip+size<total;
    }
    private void Move(int delta) { current+=delta; Update(total); changed(current); }
}
