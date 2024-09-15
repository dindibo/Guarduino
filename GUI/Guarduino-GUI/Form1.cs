using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Text;

namespace Guarduino_GUI
{
    enum SENSOR_TYPE
    {
        SENSOR_VOLUME,
        SENSOR_LASER,
    };

    public class Sensor
    {
        public string SensorType { get; set; }
        public string SensorName { get; set; }
        public string State { get; set; }
    }

    public partial class Form1 : Form
    {
        List<Control> sensorsGui = new List<Control>();
        private int timeCounter = 0;
        private int armingCooldown = 0;
        const int COOLDOWN_TIME = 5;

        public String g_JWT_Token = "";

        enum STATE
        {
            NOT_AUTH,
            DISARMED,
            ARMED
        }

        STATE state = STATE.NOT_AUTH;

        public Form1()
        {
            this.SetStyle(
            System.Windows.Forms.ControlStyles.UserPaint |
            System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
            System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
            true);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.Hide();
            var auth = new AuthenticationForm();
            auth.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void updateGui(STATE s)
        {
            switch (s)
            {
                case STATE.DISARMED:
                    this.label1.Visible = false;
                    this.button1.Visible = false;
                    this.label2.Visible = true;

                    this.button3.Text = "ARM";
                    this.button3.Visible = true;
                    this.button3.ForeColor = Color.DarkRed;
                    break;

                case STATE.ARMED:
                    this.label1.Visible = false;
                    this.button1.Visible = false;
                    this.label2.Visible = true;

                    this.button3.Text = "DISARM";
                    this.button3.Visible = true;
                    this.button3.ForeColor = Color.ForestGreen;
                    break;

                case STATE.NOT_AUTH:
                    break;

                default:
                    break;
            }
        }

        void chageState(STATE s)
        {
            this.state = s;
            updateGui(s);

            switch(s)
            {
                case STATE.DISARMED:
                    break;

                case STATE.ARMED:
                    //GetSensors();
                    break;

                case STATE.NOT_AUTH:
                    break;

                default:
                    break;
            }
        }

        public static List<Sensor>? GetSensors()
        {
            string ip, path, jwtToken;

            ip = Authentication.ip_address;
            jwtToken = Authentication.Jwt_Token;
            path = "zones";

            using (HttpClient client = new HttpClient())
            {
                // Set the timeout to 2 seconds
                client.Timeout = TimeSpan.FromSeconds(2);

                // Create the URL for the GET request
                string requestUrl = $"http://{ip}:5000/{path}";

                // Set the JWT token in the Cookie header
                client.DefaultRequestHeaders.Add("Cookie", $"jwt_token={jwtToken}");


                try
                {
                    // Make the GET request synchronously
                    HttpResponseMessage response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read and deserialize the JSON response
                    string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // Parse the JSON response into a JObject
                    JObject json = JObject.Parse(jsonResponse);

                    // Extract the list of sensors from the JSON
                    JArray sensorsArray = (JArray)json["sensors"];

                    // Create a list to hold the sensors
                    List<Sensor> sensors = new List<Sensor>();

                    // Iterate over each element in the array
                    foreach (JObject sensorObject in sensorsArray)
                    {
                        // Create a Sensor object and populate it with data
                        Sensor sensor = new Sensor
                        {
                            SensorType = sensorObject["sensor_type"]?.ToString(),
                            SensorName = sensorObject["sensor_name"]?.ToString(),
                            State = sensorObject["state"]?.ToString()
                        };

                        // Add the sensor to the list
                        sensors.Add(sensor);
                    }

                    return sensors;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return null;
                }
            }
        }

        void fetchZones()
        {
            using (HttpClient client = new HttpClient())
            {
                // Set the timeout to 1 second
                client.Timeout = TimeSpan.FromSeconds(1);

                // Create the URL for the auth endpoint
                string url = $"http://{Authentication.ip_address}:5000/zones";

                try
                {
                    // Make the POST request to the MCU auth endpoint (synchronously)
                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Get the JWT token from the response cookies
                    var cookies = response.Headers.GetValues("Set-Cookie");

                    foreach (var cookie in cookies)
                    {
                        if (cookie.Contains("jwt_token"))
                        {
                            string jwtToken = cookie.Substring(cookie.IndexOf("jwt_token=") + 10);
                            jwtToken = jwtToken.Substring(0, jwtToken.IndexOf(";"));
                            // 1111
                        }
                    }

                    return;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (HttpRequestException e)
                {
                    return;
                }
                catch (SocketException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    return;
                }
            }

        }

        void CreateSensorBox(Point p, Sensor sensor, int size)
        {
            Label lab;

            lab = new Label();
            lab.Location = p;
            lab.Visible = true;
            lab.Text = String.Format("{0}", sensor.SensorName);
            lab.ForeColor = Color.White; 
            lab.BackColor = sensor.State == "open" ? Color.DimGray : Color.Red; 
            lab.Size = new Size(size, size);

            Controls.Add(lab);
            sensorsGui.Add(lab);
        }

        void updateZonesGui(List<Sensor> sensors)
        {
            const int STEP = 100;
            const int MARGIN = STEP / 2;
            int createdSensors = 0;

            Point p = new Point(200, 300);

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if(createdSensors < sensors.Count)
                    {
                        CreateSensorBox(p, sensors[createdSensors], STEP);
                        createdSensors++;
                    }
                    else
                    {
                        goto after;
                    }
                    p.X += STEP + MARGIN;
                }
                p.Y += STEP + MARGIN;
                p.X -= (STEP + MARGIN) * 5;
            }
        after:

            return;
        }

        void destroySensors()
        {
            if (sensorsGui.Count > 0)
            {
                foreach (Control c in sensorsGui)
                {
                    Controls.Remove(c);
                }

                sensorsGui.Clear();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timeCounter++;

            if (Authentication.Jwt_Token != "" && state == STATE.NOT_AUTH && AuthenticationForm.windowCounter == 0)
            {
                chageState(STATE.DISARMED);
            }

            // Every 1 second
            if(timeCounter % 10 == 0)
            {
                if (state == STATE.ARMED)
                {
                    if (armingCooldown <= 0)
                    {
                        List<Sensor>? sensors = GetSensors();
                        if (sensors != null)
                        {
                            destroySensors();
                            updateZonesGui(sensors);
                        }  
                    }
                    else
                    {
                        if(--armingCooldown == 0)
                        {
                            label2.Text = "Connected";
                        }
                        else
                        {
                            label2.Text = String.Format("Arming in {0} seconds...", armingCooldown);
                        }
                    }
                }
                else if(state == STATE.DISARMED)
                {
                    destroySensors();
                    armingCooldown = COOLDOWN_TIME;
                    label2.Text = "Connected";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Arming
            if(this.state == STATE.DISARMED)
            {
                chageState(STATE.ARMED);
            }
            // Dis-Arming
            else if(this.state == STATE.ARMED)
            {
                chageState(STATE.DISARMED);
            }
        }
    }
}