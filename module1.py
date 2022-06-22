
urls = ["https://www.youtube.com/feed/subscriptions"]
import webbrowser
import time

#------------------------------------------------------------------
def open_tabs():
    start_url= urls[0]
    browser_path = '"C:\Program Files\Google\Chrome\Application\chrome.exe" --incognito %s'
    browser_controller=webbrowser.get(browser_path)
    #webbrowser.open(start_url)

    for i in range(0,len(urls)):
        browser_controller.open(urls[i],2)


#------------------------------------------------------------------
open_tabs()