<html>
<body>
<!--StartFragment--><html><head></head><body><h1>RaspiLedOkWeb Setup Guide</h1>
<p>A web application for controlling LED displays on Raspberry Pi and other ARM64 Linux systems.</p>
<h2>Prerequisites</h2>
<ul>
<li>Linux ARM64 system (Raspberry Pi 4, Pi 5, or compatible ARM64 device)</li>
<li>Root or sudo access</li>
<li>Internet connection for initial setup</li>
</ul>
<h2>Quick Installation</h2>
<h3>Step 1: Download and Extract</h3>
<ol>
<li>Download the <code>linux-arm64</code> binary to your system</li>
<li>Create the application directory and move the binary:</li>
</ol>
<pre><code class="language-bash">
sudo mv linux-arm64 /home/snyder/Desktop
dos2unix setup-service.sh
sudo ln -s /home/snyder/.dotnet/dotnet /usr/bin/dotnet
</code></pre>
<h3>Step 2: Service Setup</h3>
<ol>
<li>Navigate to the application directory:</li>
</ol>
<pre><code class="language-bash">cd /home/snyder/Desktop
</code></pre>
<ol start="2">
<li>
<p><strong>Important</strong>: Make sure you have the <code>setup-service.sh</code> file in this directory. If it's missing, you need to download it from your project repository or create it with the service configuration.</p>
</li>
<li>
<p>Check if the setup script exists:</p>
</li>
</ol>
<pre><code class="language-bash">ls -la setup-service.sh
</code></pre>
<ol start="4">
<li>Make the setup script executable:</li>
</ol>
<pre><code class="language-bash">chmod +x setup-service.sh
</code></pre>
<ol start="5">
<li>Run the setup script:</li>
</ol>
<pre><code class="language-bash">./setup-service.sh
</code></pre>
<p>If you get "command not found", try:</p>
<pre><code class="language-bash">bash setup-service.sh
</code></pre>
<p>The service will be automatically installed and started.</p>
<h2>Manual Configuration</h2>
<p>If you need to customize the application settings, edit the configuration variables in <code>setup-service.sh</code> before running it:</p>
<pre><code class="language-bash">nano setup-service.sh
</code></pre>
<h3>Configuration Variables</h3>
<p>Edit these values in the setup script according to your needs:</p>
<pre><code class="language-bash"># Configuration - EDIT THESE VALUES
APP_NAME="RaspiLedOkWeb"
APP_PATH="/home/RaspiLedOkWeb"
APP_EXECUTABLE="RaspiLedOkWeb"
SERVICE_PORT="5000"
ENVIRONMENT="Production"
</code></pre>

Variable | Description | Default Value
-- | -- | --
APP_NAME | Name of the application and service | RaspiLedOkWeb
APP_PATH | Full path to application directory | /home/RaspiLedOkWeb
APP_EXECUTABLE | Name of the executable file | RaspiLedOkWeb
SERVICE_PORT | Port the web application will run on | 5000
ENVIRONMENT | Application environment mode | Production


<h3>Custom Installation Path</h3>
<p>If you want to install to a different location, update both the file placement and the configuration:</p>
<ol>
<li>Create your custom directory:</li>
</ol>
<pre><code class="language-bash">sudo mkdir -p /your/custom/path
</code></pre>
<ol start="2">
<li>Move the binary:</li>
</ol>
<pre><code class="language-bash">sudo mv linux-arm64 /your/custom/path/RaspiLedOkWeb
sudo chmod +x /your/custom/path/RaspiLedOkWeb
</code></pre>
<ol start="3">
<li>Update <code>setup-service.sh</code>:</li>
</ol>
<pre><code class="language-bash">APP_PATH="/your/custom/path"
</code></pre>
<h2>Service Management</h2>
<p>After installation, you can manage the service using systemctl:</p>
<h3>Check service status:</h3>
<pre><code class="language-bash">sudo systemctl status RaspiLedOkWeb
</code></pre>
<h3>Start the service:</h3>
<pre><code class="language-bash">sudo systemctl start RaspiLedOkWeb
</code></pre>
<h3>Stop the service:</h3>
<pre><code class="language-bash">sudo systemctl stop RaspiLedOkWeb
</code></pre>
<h3>Restart the service:</h3>
<pre><code class="language-bash">sudo systemctl restart RaspiLedOkWeb
</code></pre>
<h3>Enable auto-start on boot:</h3>
<pre><code class="language-bash">sudo systemctl enable RaspiLedOkWeb
</code></pre>
<h3>View service logs:</h3>
<pre><code class="language-bash">sudo journalctl -u RaspiLedOkWeb -f
</code></pre>
<h2>Accessing the Application</h2>
<p>Once the service is running, you can access the web interface at:</p>
<pre><code>http://your-raspberry-pi-ip:5000
</code></pre>
<p>Or on the local machine:</p>
<pre><code>http://localhost:5000
</code></pre>
<h2>Troubleshooting</h2>
<h3>Permission Issues</h3>
<p>If you encounter permission errors:</p>
<pre><code class="language-bash">sudo chown -R $USER:$USER /home/RaspiLedOkWeb
sudo chmod +x /home/RaspiLedOkWeb/RaspiLedOkWeb
</code></pre>
<h3>Port Already in Use</h3>
<p>If port 5000 is already in use:</p>
<ol>
<li>Edit <code>setup-service.sh</code> and change <code>SERVICE_PORT</code> to an available port</li>
<li>Re-run the setup script</li>
</ol>
<h3>Service Won't Start</h3>
<p>Check the service logs for detailed error information:</p>
<pre><code class="language-bash">sudo journalctl -u RaspiLedOkWeb --no-pager
</code></pre>
<h3>Firewall Issues</h3>
<p>If you can't access the web interface from other devices, check your firewall:</p>
<pre><code class="language-bash">sudo ufw allow 5000
</code></pre>
<h2>File Structure</h2>
<p>After installation, your directory structure should look like:</p>
<pre><code>/home/RaspiLedOkWeb/
├── RaspiLedOkWeb          # Main executable
├── setup-service.sh       # Service setup script
└── [other application files]
</code></pre>
<h2>Uninstallation</h2>
<p>To remove the application and service:</p>
<ol>
<li>Stop and disable the service:</li>
</ol>
<pre><code class="language-bash">sudo systemctl stop RaspiLedOkWeb
sudo systemctl disable RaspiLedOkWeb
</code></pre>
<ol start="2">
<li>Remove the service file:</li>
</ol>
<pre><code class="language-bash">sudo rm /etc/systemd/system/RaspiLedOkWeb.service
sudo systemctl daemon-reload
</code></pre>
<ol start="3">
<li>Remove application files:</li>
</ol>
<pre><code class="language-bash">sudo rm -rf /home/RaspiLedOkWeb
</code></pre>
<h2>Support</h2>
<p>For issues or questions:</p>
<ul>
<li>Check the application logs using <code>journalctl</code></li>
<li>Verify all file permissions are correct</li>
<li>Ensure the ARM64 binary is compatible with your system</li>
<li>Check that the specified port is available and not blocked by firewall</li>
</ul>
<hr>
<p><strong>Note</strong>: Make sure your system supports ARM64 binaries and has all necessary dependencies installed before running the application.</p></body></html><!--EndFragment-->
</body>
</html>
