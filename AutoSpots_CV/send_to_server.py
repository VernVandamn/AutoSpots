﻿import socket

def sendDataToServer(input):

	host ="autospots.otzo.com"
	port = 22032
	message = ''.join(str(s) for s in input)
	message = 'cv_' + message;

	s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

	s.connect((host,port))

	#r = input(str)

	s.send(message.encode('ascii'))

	data = s.recv(1024)

	s.close()

	print 'received data:',  data

'''
	# Testing writing and reading to files
	v = []
	for x in range(1,10):
		v.append(1)

	print v

	with open('otherSendData', 'w+') as file:
		str = ','.join(str(e) for e in v)
		print str
		file.write(str)

	with open('otherSendData', 'r') as f:
		read_data = f.read()
		print read_data
'''
