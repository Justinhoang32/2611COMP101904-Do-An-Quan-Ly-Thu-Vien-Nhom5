using System.Globalization;
using System.Text;

namespace QuanLyThuVien.Demo;

// Du lieu UI minh hoa, khong ket noi hoac ghi vao database cua nhom.
public sealed class DemoBook
{
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Category { get; set; } = "";
    public string Location { get; set; } = "";
    public int Year { get; set; } = 2023;
    public int Total { get; set; }
    public int Available { get; set; }
    public bool Borrowable { get; set; } = true;
    public byte[]? CoverImage { get; set; }
}

public sealed class DemoReader
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "Sinh viên";
    public string Faculty { get; set; } = "Tâm lý học";
    public string StudentId { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime Issued { get; set; } = new(2026, 9, 1);
    public DateTime Expires { get; set; } = new(2027, 9, 30);
}

public sealed class DemoLoanLine
{
    public DemoBook Book { get; set; } = new();
    public string CopyCode { get; set; } = "";
    public DateTime? Returned { get; set; }
    public string Condition { get; set; } = "Tốt";
    public string Note { get; set; } = "";
}

public sealed class DemoLoan
{
    public string Code { get; set; } = "";
    public DemoReader Reader { get; set; } = new();
    public DateTime Borrowed { get; set; }
    public DateTime Due { get; set; }
    public string Note { get; set; } = "";
    public string Staff { get; set; } = "Nguyễn Lan Hương";
    public List<DemoLoanLine> Lines { get; set; } = [];
    public int Remaining => Lines.Count(x => x.Returned == null);
    public int RemainingAt(DateTime date) => Lines.Count(x => x.Returned == null || x.Returned.Value.Date > date.Date);
    public int LateDays(DateTime date) => Math.Max(0, (date.Date - Due.Date).Days);
    public string Status(DateTime date) => Remaining == 0 ? "Đã trả" : Due.Date < date.Date
        ? $"Trễ {LateDays(date)} ngày" : Remaining < Lines.Count ? "Trả một phần" : "Đang mượn";
}

public sealed class DemoCatalogItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class DemoSession
{
    public string Name { get; set; } = "Nguyễn Lan Hương";
    public bool IsManager { get; set; }
}

public sealed class LibraryDemo
{
    public DateTime Today { get; } = new(2026, 11, 2);
    public List<DemoBook> Books { get; } = [];
    public List<DemoReader> Readers { get; } = [];
    public List<DemoLoan> Loans { get; } = [];
    public List<DemoCatalogItem> Authors { get; } = [];
    public List<DemoCatalogItem> Categories { get; } = [];
    public DemoSession Session { get; } = new();
    public Dictionary<string, int> Draft { get; } = new();
    public string DraftReaderCode { get; set; } = "DG0002";
    public DateTime DraftBorrowed { get; set; } = new(2026, 11, 2);
    public DateTime DraftDue { get; set; } = new(2026, 11, 16);
    public string DraftNote { get; set; } = "Mượn phục vụ học phần Tâm lý học.";
    private int sequence = 16;

    public LibraryDemo()
    {
        string[] titles = ["Tâm lý học đại cương", "Giáo dục học", "Cơ sở văn hóa Việt Nam", "Dạy học phát triển năng lực", "Đại số tuyến tính", "Ngôn ngữ học đại cương", "Cơ sở dữ liệu", "Văn học Việt Nam", "Lịch sử Việt Nam", "Lập trình C#", "Giáo dục học đại cương", "Mạng máy tính"];
        string[] authors = ["Nguyễn Quang Uẩn", "Phạm Viết Vượng", "Trần Ngọc Thêm", "Đỗ Hương Trà", "Nguyễn Hữu Việt Hưng", "Đỗ Hữu Châu", "Nguyễn Văn Ba", "Nguyễn Nhật Ánh", "Trần Trọng Kim", "Lê Văn Thái", "Đặng Vũ Hoạt", "Nguyễn Đức Thuần"];
        string[] cats = ["Tâm lý học", "Giáo dục học", "Văn hóa", "Giáo dục học", "Toán học", "Ngôn ngữ học", "Tin học", "Văn học", "Lịch sử", "Tin học", "Giáo dục học", "Tin học"];
        string[] locations = ["TL · A01", "GD · B02", "VH · A03", "GD · B03", "TH · C01", "NN · B04", "TH · C02", "VH · D01", "LS · A05", "TH · C03", "GD · B05", "TH · C04"];
        int[] totals = [24,18,15,12,20,16,12,10,7,8,13,6];
        int[] available = [8,5,1,0,12,6,9,3,0,4,2,0];
        for (int i = 0; i < titles.Length; i++)
        {
            Books.Add(new() { Code = $"S{i+1:D3}", Title = titles[i], Author = authors[i], Category = cats[i], Location = locations[i], Total = totals[i], Available = available[i] });
            Authors.Add(new() { Code = $"TG{i+1:D3}", Name = authors[i] });
        }
        foreach (var cat in cats.Distinct()) Categories.Add(new() { Code = $"TL{Categories.Count+1:D3}", Name = cat });
        Authors[2].Description = "Nhà nghiên cứu văn hóa, tác giả các công trình về bản sắc văn hóa Việt Nam.";
        Categories[2].Description = "Cơ sở văn hóa, văn hóa Việt Nam và nghiên cứu văn hóa học.";
        string[] names = ["Nguyễn Minh Anh", "Trần Hoàng Nam", "Lê Thảo Nguyên", "Phạm Gia Hân", "Võ Ngọc Bảo", "Đặng Quốc Huy", "Nguyễn Khánh Linh", "Trần Bảo Ngọc", "Lê Minh Khang", "Huỳnh Thu Hà", "Đỗ Phương Thảo", "Phạm Quốc An", "Nguyễn Ngọc Hân", "Trần Hữu Phúc", "Lê Hải Yến", "Vũ Minh Đức"];
        for (int i = 0; i < names.Length; i++)
            Readers.Add(new() { Code = $"DG{i+1:D4}", Name = names[i], Group = i is 2 or 12 ? "Giảng viên" : i == 5 ? "Cán bộ" : "Sinh viên", StudentId = $"49.01.103.{i+1:D3}", Phone = i == 0 ? "0903 456 781" : $"0908 221 {436+i:D3}", Email = $"docgia{i+1}@example.com" });
        Readers[4].Expires = Today.AddDays(6); Readers[5].Expires = Today.AddDays(-2);
        AddLoan("PM261102-015", 0, Today, Today.AddDays(14), [0,1,2], "Mượn phục vụ học phần Tâm lý học.");
        Loans[0].Lines[0].CopyCode = "S001-017"; Loans[0].Lines[1].CopyCode = "S002-009"; Loans[0].Lines[2].CopyCode = "S003-012";
        AddLoan("PM261102-014", 1, Today, Today.AddDays(14), [0,4]);
        AddLoan("PM261101-009", 2, Today.AddDays(-1), Today.AddDays(13), [1]);
        Loans[^1].Lines[0].Returned = Today;
        AddLoan("PM261028-021", 4, new(2026,10,28), new(2026,11,11), [2,5]);
        Loans[^1].Lines[0].Returned = Today;
        AddLoan("PM261020-012", 5, new(2026,10,20), new(2026,11,3), [0,1,4,5]);
        int[] days = [4,7,5,9,1,2,14,6,8,3,10,11];
        int[][] indexes = [[0,2,5],[1,3],[2,5],[0,0,1,4],[1],[3,5],[0,4,5],[1,2],[3,4],[0,5],[1,4],[0,1,5]];
        int[] readerIndexes = [3,6,7,8,9,10,11,12,13,14,15,2];
        for (int i = 0; i < days.Length; i++)
        {
            var due = Today.AddDays(-days[i]); var borrowed = due.AddDays(-14);
            AddLoan($"PM{borrowed:yyMMdd}-{i+1:D3}", readerIndexes[i], borrowed, due, indexes[i]);
        }
        Loans[5].Code = "PM261015-008"; Loans[5].Borrowed = new(2026,10,15);
        Draft["S001"] = 1; Draft["S002"] = 1; Draft["S003"] = 2;
    }

    private void AddLoan(string code, int reader, DateTime borrowed, DateTime due, int[] books, string note = "")
        => Loans.Add(new() { Code = code, Reader = Readers[reader], Borrowed = borrowed, Due = due, Note = note,
            Lines = books.Select((b,i) => new DemoLoanLine { Book = Books[b], CopyCode = $"{Books[b].Code}-{Loans.Count*5+i+1:D3}" }).ToList() });

    public static bool Matches(string? value, string? term) => string.IsNullOrWhiteSpace(term) ||
        SearchKey(value ?? "").Contains(SearchKey(term.Trim()), StringComparison.OrdinalIgnoreCase);

    private static string SearchKey(string text)
    {
        var key = new StringBuilder();
        foreach (char c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                key.Append(c is 'đ' or 'Đ' ? 'd' : c);
        return key.ToString();
    }

    public IEnumerable<DemoBook> Search(string code, string title, string author, string category, int state, bool available)
        => Books.Where(b => Matches(b.Code, code) && Matches(b.Title, title) && Matches(b.Author, author)
            && (category == "Tất cả thể loại" || b.Category == category)
            && (state == 0 || b.Borrowable == (state == 1)) && (!available || b.Available > 0)).OrderBy(b => b.Code);

    public IEnumerable<DemoLoan> Overdue(DateTime date, string group = "Tất cả nhóm", int minDays = 0, int maxDays = int.MaxValue, string search = "")
        => Loans.Where(l => l.Borrowed.Date <= date.Date && l.Due.Date < date.Date && l.RemainingAt(date) > 0
            && (group == "Tất cả nhóm" || l.Reader.Group == group) && l.LateDays(date) >= minDays && l.LateDays(date) <= maxDays
            && (Matches(l.Code, search) || Matches(l.Reader.Name, search) || Matches(l.Reader.Code, search)));

    public List<string> ValidateDraft(DemoReader? reader, DateTime borrowed, DateTime due)
    {
        List<string> errors = [];
        if (reader == null) errors.Add("Vui lòng chọn độc giả.");
        else
        {
            if (!Readers.Contains(reader)) errors.Add("Độc giả không thuộc danh mục.");
            if (reader.Issued.Date > borrowed.Date) errors.Add("Thẻ độc giả chưa đến ngày hiệu lực.");
            if (reader.Expires.Date < borrowed.Date) errors.Add("Thẻ độc giả đã hết hạn.");
            if (Overdue(borrowed).Any(x => x.Reader.Code == reader.Code)) errors.Add("Độc giả đang có phiếu quá hạn.");
            if (Loans.Where(x => x.Reader.Code == reader.Code).Sum(x => x.Remaining) + Draft.Values.Sum() > 5)
                errors.Add("Tổng số sách đang mượn và đang chọn vượt hạn mức 5 cuốn.");
        }
        if (borrowed.Date > Today) errors.Add("Ngày mượn không được sau ngày đối chiếu.");
        if (due.Date < borrowed.Date) errors.Add("Ngày hẹn trả phải từ ngày mượn trở đi.");
        if (Draft.Count == 0) errors.Add("Phiếu chưa có sách.");
        foreach (var (code, count) in Draft)
        {
            var book = Books.FirstOrDefault(x => x.Code == code);
            if (book == null || !book.Borrowable || count < 1 || count > book.Available)
                errors.Add($"{code}: số lượng không hợp lệ hoặc vượt tồn kho.");
        }
        return errors;
    }

    public DemoLoan CreateLoan(DemoReader reader, DateTime borrowed, DateTime due, string note)
    {
        var errors = ValidateDraft(reader, borrowed, due);
        if (errors.Count != 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        var loan = new DemoLoan { Code = $"PM{borrowed:yyMMdd}-{sequence++:D3}", Reader = reader, Borrowed = borrowed.Date, Due = due.Date, Note = note, Staff = Session.Name };
        foreach (var (code, count) in Draft)
        {
            var book = Books.Single(b => b.Code == code);
            for (int i = 0; i < count; i++) loan.Lines.Add(new() { Book = book, CopyCode = $"{code}-M{sequence:D3}-{i+1}" });
            book.Available -= count;
        }
        Loans.Insert(0, loan); Draft.Clear(); return loan;
    }

    public void ReturnBooks(DemoLoan loan, IReadOnlyList<(DemoLoanLine Line, string Condition, string Note)> selected, DateTime date)
    {
        if (!Loans.Contains(loan)) throw new InvalidOperationException("Phiếu mượn không thuộc danh sách.");
        if (selected.Count == 0) throw new InvalidOperationException("Chọn ít nhất một cuốn sách.");
        if (date.Date < loan.Borrowed.Date || date.Date > Today) throw new InvalidOperationException("Ngày trả phải nằm từ ngày mượn đến ngày đối chiếu.");
        if (selected.Select(x => x.Line).Distinct().Count() != selected.Count) throw new InvalidOperationException("Một bản sách chỉ được ghi nhận một lần.");
        foreach (var item in selected)
        {
            if (!loan.Lines.Contains(item.Line) || item.Line.Returned != null) throw new InvalidOperationException("Có bản sách đã được trả trước đó.");
            if (item.Condition is not ("Tốt" or "Hư hỏng" or "Mất")) throw new InvalidOperationException("Tình trạng sách không hợp lệ.");
            if (item.Condition != "Tốt" && string.IsNullOrWhiteSpace(item.Note)) throw new InvalidOperationException("Sách hư hỏng hoặc mất cần ghi rõ lý do.");
        }
        foreach (var item in selected)
        {
            item.Line.Returned = date.Date; item.Line.Condition = item.Condition; item.Line.Note = item.Note;
            if (item.Condition == "Tốt") item.Line.Book.Available++;
        }
    }
}
