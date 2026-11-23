
<%@ Page Language="C#" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.IO" %>

<script runat="server">

    string conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SmartCampusDB;Integrated Security=True";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CreateTable();
            LoadRequests();
        }
    }

    // MODEL / DATABASE
    void CreateTable()
    {
        using (SqlConnection con = new SqlConnection(conStr))
        {
            con.Open();

            string sql = @"IF OBJECT_ID('CampusRequests','U') IS NULL
            CREATE TABLE CampusRequests(
                Id INT IDENTITY(1,1) PRIMARY KEY,
                StudentName VARCHAR(100),
                Service VARCHAR(100),
                Description VARCHAR(500),
                Status VARCHAR(50) DEFAULT 'Pending'
            )";

            new SqlCommand(sql, con).ExecuteNonQuery();
        }
    }

    // CONTROLLER - ADD REQUEST
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        using (SqlConnection con = new SqlConnection(conStr))
        {
            string sql = @"INSERT INTO CampusRequests
            (StudentName,Service,Description)
            VALUES(@name,@service,@description)";

            SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@name", txtName.Text);
            cmd.Parameters.AddWithValue("@service", ddlService.SelectedValue);
            cmd.Parameters.AddWithValue("@description", txtDescription.Text);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        lblMessage.Text = "Request submitted successfully!";
        txtName.Text = "";
        txtDescription.Text = "";

        LoadRequests();
    }

    // VIEW REQUESTS
    void LoadRequests()
    {
        using (SqlConnection con = new SqlConnection(conStr))
        {
            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM CampusRequests ORDER BY Id DESC", con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            gvRequests.DataSource = dt;
            gvRequests.DataBind();
        }
    }

    // UPDATE STATUS
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        using (SqlConnection con = new SqlConnection(conStr))
        {
            string sql = @"UPDATE CampusRequests
            SET Status=@status WHERE Id=@id";

            SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@status", ddlStatus.SelectedValue);
            cmd.Parameters.AddWithValue("@id", txtRequestId.Text);

            con.Open();

            int rows = cmd.ExecuteNonQuery();

            lblMessage.Text = rows > 0
                ? "Request status updated!"
                : "Request ID not found.";
        }

        LoadRequests();
    }

    // API INTEGRATION
    protected void btnWeather_Click(object sender, EventArgs e)
    {
        try
        {
            using (WebClient client = new WebClient())
            {
                string url = "https://wttr.in/Rajkot?format=3";
                string result = client.DownloadString(url);

                lblWeather.Text = "Campus Weather: " + result;
            }
        }
        catch
        {
            lblWeather.Text = "Weather API unavailable.";
        }
    }

</script>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Smart Campus Service Portal</title>

    <style>
        body {
            font-family: Arial;
            background: #eef2f7;
            margin: 0;
        }

        .header {
            background: #173b70;
            color: white;
            padding: 25px;
            text-align: center;
        }

        .container {
            width: 85%;
            margin: 30px auto;
        }

        .card {
            background: white;
            padding: 25px;
            margin-bottom: 25px;
            border-radius: 10px;
            box-shadow: 0 3px 10px #ccc;
        }

        input, select, textarea {
            width: 95%;
            padding: 10px;
            margin: 10px 0;
            border: 1px solid #ccc;
            border-radius: 5px;
        }

        .btn {
            background: #173b70;
            color: white;
            padding: 12px 22px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }

        .btn:hover {
            background: #2863a5;
        }

        .grid {
            width: 100%;
            margin-top: 20px;
            border-collapse: collapse;
        }

        .grid th {
            background: #173b70;
            color: white;
        }

        .grid td, .grid th {
            padding: 12px;
            border: 1px solid #ddd;
        }

        .message {
            color: green;
            font-weight: bold;
        }
    </style>
</head>

<body>

<form id="form1" runat="server">

<div class="header">
    <h1>Smart Campus Service Portal</h1>
    <p>One Platform for All Campus Services</p>
</div>

<div class="container">

    <div class="card">
        <h2>Student Service Request</h2>

        <asp:TextBox ID="txtName" runat="server"
            placeholder="Enter Student Name"></asp:TextBox>

        <asp:DropDownList ID="ddlService" runat="server">
            <asp:ListItem>Library Service</asp:ListItem>
            <asp:ListItem>Hostel Service</asp:ListItem>
            <asp:ListItem>Transport Service</asp:ListItem>
            <asp:ListItem>IT Support</asp:ListItem>
            <asp:ListItem>Maintenance</asp:ListItem>
        </asp:DropDownList>

        <asp:TextBox ID="txtDescription" runat="server"
            TextMode="MultiLine"
            Rows="4"
            placeholder="Describe your issue"></asp:TextBox>

        <br />

        <asp:Button ID="btnSubmit" runat="server"
            Text="Submit Request"
            CssClass="btn"
            OnClick="btnSubmit_Click" />

        <br /><br />

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message"></asp:Label>
    </div>

    <div class="card">
        <h2>Manage Request Status</h2>

        <asp:TextBox ID="txtRequestId" runat="server"
            placeholder="Enter Request ID"></asp:TextBox>

        <asp:DropDownList ID="ddlStatus" runat="server">
            <asp:ListItem>Pending</asp:ListItem>
            <asp:ListItem>In Progress</asp:ListItem>
            <asp:ListItem>Completed</asp:ListItem>
        </asp:DropDownList>

        <asp:Button ID="btnUpdate" runat="server"
            Text="Update Status"
            CssClass="btn"
            OnClick="btnUpdate_Click" />
    </div>

    <div class="card">
        <h2>All Campus Requests</h2>

        <asp:GridView ID="gvRequests" runat="server"
            CssClass="grid"
            AutoGenerateColumns="true"
            EmptyDataText="No requests available">
        </asp:GridView>
    </div>

    <div class="card">
        <h2>Campus Weather (External API)</h2>

        <asp:Button ID="btnWeather" runat="server"
            Text="Check Weather"
            CssClass="btn"
            OnClick="btnWeather_Click" />

        <br /><br />

        <asp:Label ID="lblWeather" runat="server"></asp:Label>
    </div>

</div>

</form>

</body>
</html>
