using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Net.Sockets;

namespace Guarduino_GUI
{
    public partial class AuthenticationForm : Form
    {
        public static int windowCounter = 0;

        public AuthenticationForm()
        {
            windowCounter++;
            this.FormClosed += AuthenticationForm_FormClosed;

            InitializeComponent();
        }

        private void AuthenticationForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            windowCounter--;
        }

        private void Authentication_Load(object sender, EventArgs e)
        {
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
            //this.TopMost = true;
        }

        private void Authentication_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        public string Authenticate(string user, string password, string ip)
        {
            using (HttpClient client = new HttpClient())
            {
                // Set the timeout to 1 second
                client.Timeout = TimeSpan.FromSeconds(1);

                // Create the URL for the auth endpoint
                string url = $"http://{ip}:5000/auth";

                // Create the request payload
                var payload = new
                {
                    username = user,
                    password = password
                };

                // Serialize the payload into JSON using Newtonsoft.Json
                string jsonPayload = JsonConvert.SerializeObject(payload);

                // Prepare the HTTP content with JSON format
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // Make the POST request to the MCU auth endpoint (synchronously)
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();

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
                            return jwtToken;
                        }
                    }

                    return "JWT token not found";
                }
                catch (TaskCanceledException)
                {
                    return "Request timed out after 1 second.";
                }
                catch (HttpRequestException e)
                {
                    return $"Request error: {e.Message}";
                }
                catch (SocketException)
                {
                    return "Invalid IP address.";
                }
                catch (Exception ex)
                {
                    return $"Unexpected error: {ex.Message}";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string jwtToken = "";

            try
            {
                jwtToken = Authenticate(this.textBox1.Text, this.textBox2.Text, this.textBox3.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return;
            }

            if (jwtToken == "Timeout")
            {
                MessageBox.Show("Request timed out.");
            }
            else if (jwtToken == "JWT token not found")
            {
                MessageBox.Show("JWT token not found");
            }
            else if (!string.IsNullOrEmpty(jwtToken))
            {
                Authentication.Jwt_Token = jwtToken;
                Authentication.username = this.textBox1.Text;
                Authentication.password = this.textBox2.Text;
                Authentication.ip_address = this.textBox3.Text;

                MessageBox.Show("Authentication succsess");
                this.Close();
                return;
            }
            else
            {
                MessageBox.Show("Authentication failed.");
            }

            Authentication.Jwt_Token = "";
        }

    }
}
