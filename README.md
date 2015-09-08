Produced by Joel R. Evans @ JoelREvans.com

# Google-Domains-Dynamic-DNS-Updater
Windows service that checks your server's IP for changes and updates the DNS records in google domains.  
Intended for free & public use.

There isn't a directory installer for this service.  Just drag it into the directory you want it to reisde.
To install the service itself, run the .exe in the release folder, and follow the prompts.  You can tweak 
settings about how you want it to start in services.msc.

The interval at which the service checks and updates the IP may be set in the settings.settings file.
To add a new domain, update the XML in the settings file.  You need to enter the username and password provided by 
google domains, then simply provide the hostname.
