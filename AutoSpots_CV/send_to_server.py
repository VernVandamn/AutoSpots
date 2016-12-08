import socket

def sendDataToServer(input):

    str = ','.join(str(s) for s in input)

    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    host ="192.168.1.3"
    port =8000

    s.connect((host,port))

    r = input(str)

    s.send(r.encode())

    #We probably don't care about the response
    #data = ''
    #data = s.recv(1024).decode()
    #print (data)

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