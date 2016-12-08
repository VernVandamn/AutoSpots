import socket

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
host ="192.168.1.3"
port =8000

'''
s.connect((host,port))

r = input('Enter message')
s.send(r.encode())
data = ''
data = s.recv(1024).decode()
print (data)
'''
