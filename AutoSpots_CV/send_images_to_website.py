import socket
import pickle

# Symbolic name meaning all available interfaces
HOST = '34.200.219.226' 
# Arbitrary port
PORT = 50007

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect((HOST, PORT))

file_name = './input.jpg'
with open(file_name, 'r') as img:
	while True:
		data = img.readline(512)
		if not data:
			break
		client_socket.send(data)
print "data sent"
exit()