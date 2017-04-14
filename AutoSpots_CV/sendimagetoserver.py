import requests
import base64

encoded_string = ''
lotID = 7
baseurl = "http://jamesljenk.pythonanywhere.com/"
response = requests.get(baseurl+"/lots/")
with open("input.jpg", "rb") as image_file:
    encoded_string = base64.encodestring(image_file.read())

response = requests.post(baseurl+'update/image/'+str(lotID)+"/", json={'data': encoded_string.decode() })

print response.conten
