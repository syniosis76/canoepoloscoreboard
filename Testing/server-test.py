import http.client
import urllib
import json

baseUrl = 'http:///' # Set destination URL here

site = 'localhost:8080'
basePath = '/'

connection = http.client.HTTPConnection(site)
try:
    #connection.set_debuglevel(1)
    
    print('Current Game')
    connection.request('GET', basePath + 'game')
    response = connection.getresponse().read().decode()
    print(response)
    
    print('Results')
    connection.request('GET', basePath + 'results')
    response = connection.getresponse().read().decode()
    print(response)
        
    print()
    print('Replace Team Names')
    postParams = { 'AT 1st': 'Expose'
        , 'AT 2nd': 'Tu Kai Taua'
        , 'AT 3rd': 'Maelstrom'
        , 'AT 4th': 'Vortex' }
    postData = json.dumps(postParams)
    connection.request('POST', basePath + 'replace-team-names', postData)
    response = connection.getresponse().read().decode()
    print(response)
        
    print('Clear Games')
    connection.request('GET', basePath + 'clear-games')
    response = connection.getresponse().read().decode()
    print(response)
    
    print()
    print('Add Game 1')
    postParams = { 'pool': 'B1' , 'team1': 'Slackers', 'team2': 'Viking Silver'
        , 'periods': [ { 'name': 'Period 1', 'startTime': '07:30', 'endTime': '07:40' }, { 'name': 'Period 2', 'startTime': '07:42', 'endTime': '07:52' } ] }        
    postData = json.dumps(postParams)
    connection.request('POST', basePath + 'add-game', postData)
    response = connection.getresponse().read().decode()
    print(response)
    
    print()
    print('Add Game 2')
    postParams = { 'pool': 'B1' , 'team1': 'Guvnors', 'team2': 'Viking Platinum'
        , 'periods': [ { 'name': 'Period 1', 'startTime': '08:00', 'endTime': '08:10' }, { 'name': 'Period 2', 'startTime': '08:12', 'endTime': '08:22' } ] }        
    postData = json.dumps(postParams)
    connection.request('POST', basePath + 'add-game', postData)
    response = connection.getresponse().read().decode()
    print(response)    
finally:
    connection.close()
