# -*- coding: utf-8 -*-
from selenium import webdriver
from selenium.webdriver.common.keys import Keys
import sys, os, os.path, time, subprocess

#####
# get credentials from config file in APPDATA
exists = os.path.isfile(os.getenv('APPDATA') + "\\shifttracker-settings.xml")
if exists:
    f = open(os.getenv('APPDATA') + "\\shifttracker-settings.xml", "r")
    username = f.readline().strip()
    password = f.readline().strip()
else:
    sys.exit(0)
#####
# clear destination folder

dlpath = os.getcwd() + "\\picagens\\"

for root, dirs, files in os.walk(dlpath):
    for file in files:
        os.remove(os.path.join(root, file))

#####
# fetch file
        
chrome_options = webdriver.ChromeOptions()
#chrome_options.add_argument("--headless")
prefs = {"download.default_directory": dlpath}
chrome_options.add_experimental_option("prefs", prefs)

driver = webdriver.Chrome(".\chromedriver.exe", options=chrome_options)

driver.get("https://intranet.tap.pt/Pages/SistemaPonto.aspx")

assert "login.tap.pt" in driver.title

usrel = driver.find_element_by_id("input_1")
usrpw = driver.find_element_by_id("input_2")
usrel.send_keys(username)
usrpw.send_keys(password)
usrpw.submit()

assert "TAP Intranet > Sistema de Ponto" in driver.title

#btnexp = driver.find_element_by_id("ctl00_ctl66_g_150d8a4a_91fb_428e_a279_e971347020e2_ctl00_PicagensControl_ExportBtn")
btnexp = driver.find_element_by_class_name("export")
btnexp.click()

time.sleep(3)

driver.close()

driver.quit()

#####
# run ShiftTracker with the given file

#os.chdir(r"shifttracker")
subprocess.Popen([r'shifttracker', r'/keepopen', r'.\picagens\Picagens.xls'])
