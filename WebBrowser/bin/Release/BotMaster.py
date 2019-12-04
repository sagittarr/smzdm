import sys
import pandas as pd
import os
import time
import subprocess
from subprocess import Popen, CREATE_NEW_CONSOLE

#os.system(cmd)
command = sys.argv[1]
inputPth = sys.argv[2]
outputPth =sys.argv[3]
print(command)
if command == "read":
    input = sys.argv[2]

    dict = {}
    with open(inputPth,'r', encoding="utf8") as myFile:
        for line in myFile.readlines():
            key = ""
            pairs = line.split(',')
            newline = {}
            for pair in pairs:
                kv = pair.split(':')
                newline[kv[0]] = kv[1]
                if kv[0] == 'username':
                    key = kv[1]
            dict[key] =  newline    
    dfObj = pd.DataFrame(columns=['username', 'error message','password','nickName','level', 'gold', 'left','loginTime','status'])  
    for kv in dict.items():
        print(kv[1])
        dfObj = dfObj.append(kv[1], ignore_index=True)
    print(dfObj)
    dfObj.to_csv(outputPth,encoding='utf-8')              
elif command == "search" or command == "crawl":
    cmd = ["./SmzdmBot.exe",command,inputPth,outputPth]
    subprocess.Popen(cmd,creationflags=CREATE_NEW_CONSOLE)
else :
    table=pd.read_csv(inputPth)
    for index, row in table.iterrows():
        comment = row['comment']
        sourceUrl = row['sourceUrl']
        login = row['login']
        if type(sourceUrl) == type("") and sourceUrl.endswith('.txt'):
            sourceUrl = 'data/' + sourceUrl
        if login!='y':
            continue
        username = ''
        password = str(row['password'])
        if str(row['email']) != "nan":
            username = str(row['email'])
        else:
            username = str(int(row['phone']))
        if sourceUrl!= "nan":
            if command == "share":
                cmd = ["./SmzdmBot.exe", command, username, password, sourceUrl,'2','10','0','23','好价',outputPth]
                print(cmd)
            elif command == "smzdm_share":
                cmd = ["./SmzdmBot.exe", command, username, password, sourceUrl,'2','5','0','23','好价',outputPth, sys.argv[4], sys.argv[5], sys.argv[6]]
            elif command == "wiki_share":
                cmd = ["./SmzdmBot.exe", command, username, password, sourceUrl,'2','5','0','23','好价',outputPth, sys.argv[4], sys.argv[5]]
            elif command == "login":
                cmd = ["./SmzdmBot.exe", command, username ,password, outputPth]
            else: 
                sys.exit()
            print(cmd)
            subprocess.Popen(cmd,creationflags=CREATE_NEW_CONSOLE)
            time.sleep(5)
