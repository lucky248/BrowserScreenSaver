# Browser Screen Saver

This is a very simple WPF-based screen saver that embeds 4 resizable browser windows. In the configuration of the screen saver you can specify the URIs for these browser windows. By default, interaction with browser is limited (navigation is disabled), but you can temporarily disable that via settings dialog.

Screen saver will run with your identity, so you can show dashboards you have access to etc. Navigation will be very constrained so much that almost no interaction is possible (no kyeboard, no right-clicks, no navigation to other sites, etc.)

Here is an [example](https://github.com/lucky248/BrowserScreenSaver/blob/master/Example.png?raw=true) of how it looks.

# Installation insructions
1. Build the code
2. Copy browser.exe into c:\Windows\System32 folder
3. Rename to browser.scr
4. Go to the control panel to "Change Screen Saver"
5. Setup URIs, monitors to your liking
6. Note that you can "Enable browser navigation for 5 mins" to deal with logins, etc. After that time expires, interaction with the browser will be limited to a point where nobody can leverage it for melicious purpouses. Most of the sites will cache the login and screensaver won't require fresh login experience.
