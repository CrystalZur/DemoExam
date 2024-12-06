var builder = WebApplication.CreateBuilder();
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

List<Req> repo = [];

bool UPD = false;
string UPDMassage = "";

app.MapGet("/", () =>
{
    if (UPD)
    {
        string buffer = UPDMassage;
        UPD = false;
        UPDMassage = "";
        return Results.Json(new UPDStatus(repo, UPDMassage));
    }
    else
        return Results.Json(repo);
});

app.MapPost("/", (Req req) => repo.Add(req));

app.MapPut("/{number}", (int number, RequestDTO o) =>
{
    Req buff = repo.Find(a => a.Number == number);
    if (buff.Status != o.Status && buff.Status != "Завершена")
    {
        buff.Status = o.Status;
        UPD = true;
        UPDMassage = "Статус заявки номер " + buff.Number + " изменен\n";
    }
    if (buff.Status == "Завершена" && buff.Status != o.Status)
    {
        buff.Status = o.Status;
        UPD = true;
        UPDMassage = "Заявка номер " + buff.Number + " завершена\n";
    }
    if (buff.Description != o.Description)
    {
        buff.Description = o.Description;
        UPD = true;
        UPDMassage = "Заявка номер " + buff.Number + " измененa\n";
    }
    if (buff.Master != o.Master)
    {
        buff.Master = o.Master;
        UPD = true;
        UPDMassage = "Заявка номер " + buff.Number + " измененa\n";
    }
    if (o.MasterNote != null)
    {
        buff.MasterNotes.Add(o.MasterNote);
        UPD = true;
        UPDMassage = "Заявка номер " + buff.Number + " измененa\n";
    }
    return Results.Json(buff);
});

app.MapGet("/doneStat", () => repo.FindAll(a => a.Status == "Выполнено"));

app.MapGet("/timeStat", () =>
{
    double TimeOv = 0;
    int Count = 0;
    foreach (var a in repo)
    {
        if (a.Status == "Выполнено")
        {
            TimeOv += a.Time();
            Count++;
        }
    }
    if (Count > 0)
    {
        return TimeOv / Count;
    }
    else
    {
        return 0;
    }
});

app.MapGet("/problemStat", () =>
{
    Dictionary<string, int> res = [];
    foreach (var a in repo)
    {
        if (res.ContainsKey(a.Description))
        {
            res[a.Description]++;
        }
        else
        {
            res[a.Description] = 1;
        }
    }
    return res;
});

app.Run();

record class UPDStatus(List<Req> repo, string UPDMassage);

record class RequestDTO (string Status, string Description, string Master, string MasterNote);

class Req
{
    int number;
    string type;
    string model;
    string description;
    string fio;
    string phoneNumber;
    string status;

    public Req(int number, int day, int month, int year, string type, string model, string description, string fio, string phoneNumber, string status)
    {
        Number = number;
        Date = new DateTime(year, month, day);
        EndDate = null;
        Type = type;
        Model = model;
        Description = description;
        Fio = fio;
        PhoneNumber = phoneNumber;
        Status = status;
        Master = "Не назначен";
    }

    public int Number { get => number; set => number = value; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
    public string Type { get => type; set => type = value; }
    public string Model { get => model; set => model = value; }
    public string Description { get => description; set => description = value; }
    public string Fio { get => fio; set => fio = value; }
    public string PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
    public string Status { get => status; set => status = value; }
    public string Master { get; set; }
    public List<string> MasterNotes { get; set; } = [];
    public double Time() => (EndDate - Date).Value.TotalDays;
}