'''
The computer vision code currently in use by the AutoSpots project originated from
https://github.com/rugbyprof/Parking-Lot-Occupancy-Tracking

This is being used as a proof of concept only
AutoSpots seek to expand this implementation
to use a Haar Cascade Classifier as well as
refining and adding to the classical computer
vision techniques employed by the current code 
here
'''

import os
import json
import sys
import urllib2
import send_to_server as serv
import platform
import AutoSpotsComputerVision as cv
import pllines as pll


spot_dir = 'Spots'
spots = {}

# TODO: json file needs to be set maybe with a cl argument
with open('newParking.json') as data_file:
	data = json.load(data_file)

black_color = (0,0,0)
red_color = (0,0,255)
green_color = (0,255,0)

#all images taken at timeOfDay

# This pulls the updated images.json file from their server but the ''.join(req) thing makes it not work
# The current images.json file works well enough

#timeOfDay = '1100'
#req = urllib2.urlopen('http://cs.mwsu.edu/~griffin/p-lot/apiproxy.php?time=' + timeOfDay)
#req = ''.join(req)

#f = open("images.json", "w")

#p_json = json.loads(req)
#f.write(p_json)
#f.close()
numImgs = 0

# TODO: this json file needs to be a cl argument as well
#use the images.json for all the images demo is just the noice ones for demo day
with open('images.json') as images_file:
# with open('demo.json') as images_file:
	images = json.load(images_file)

#loop through all images
for image in images['data']:

    # This is where you specify the number of images to scan

    # TODO: this either needs to be in the json file or delcared somewhere
    # it needs to change with each incoming file
    # if numImgs > 20:
    if numImgs > 6:
            break
    numImgs = numImgs + 1

    #url = image['shot_url']+image['camera']+'/'+image['name']
    #
    #saveImgUrl(url)

    img = cv2.imread(image['name'])

    # Show the original image first
    #cv2.imshow('image', img)
    #cv2.waitKey(0)
    #cv2.destroyWindow('image')


    p_lot = []
    parkinglotbinaryarray = []
    gray_spot = []
    gray_spot_avg = []
    spot_result = ""

    #loop through rows in json
    for row_data in data:
        #append new row to parking lot
        size = sys.getsizeof(row_data[0])

        if platform.system() == 'Darwin':
                intLength = 24
        else:
                intLength = 12
        if size == intLength: #check for gray spot
            w,h = img.shape[:2]
            pt = pll.Point(row_data)
            pt1 = pll.Point([pt.x+h/10, pt.y+w/10])
            if len(sys.argv) > 1 and 'p' in sys.argv[1]:
                    print size, h/10, w/10
            gray_spot = pll.cutParkingSpot(img, pt, pt1)
            gray_spot_avg = cv.averageColors(gray_spot)
            if len(sys.argv) > 1 and 'p' in sys.argv[1]:
                    print "Gray Spot", gray_spot_avg
            #cv2.imshow("Grayspot", gray_spot)
            #cv2.waitKey(0)
            saveImg(gray_spot, spot_dir, "1_Gray_Spot")
            spots["1_Gray_Spot"] = gray_spot
            continue #break out

            p_lot.append(
                {
                    'H': [], #list of horizontals
                    'V': [] #list of verticals
                }
            )

            #process horizontal lines  
            for h_line in row_data[0]:
                h_line_obj = pll.Line(h_line[0], h_line[1]) #get new Line object
                p_lot[-1]['H'].append(h_line_obj) #add new object to p_lot

            top_h_line = p_lot[-1]['H'][0]
            bot_h_line = p_lot[-1]['H'][1]

            #process vertical lines
        for v_line in row_data[1]:
            #create Line object for vertical line
            v_line_obj = pll.Line(
                            [v_line[0], top_h_line.get_y_intersect(v_line[0])], 
                            [v_line[1], bot_h_line.get_y_intersect(v_line[1])]
                            )

            #Start extracting spots after first vertical line
            if len(p_lot[-1]['V']) > 0:			
                #mask image
                maskedImg = cv.maskImage(img, p_lot[-1]['V'][-1], v_line_obj)
                # cv2.imshow('image', maskedImg)
                # cv2.waitKey(0)

                #get row and column of spot
                spotName = "Row_" + str(len(p_lot)) + "_Col_" + str(len(p_lot[-1]['V']))

                #extract spot from maskedImage and save
                parking_spot = pll.cutParkingSpot(
                        maskedImg, 
                        pll.find_min_point(p_lot[-1]['V'][-1], v_line_obj), 
                        pll.find_max_point(p_lot[-1]['V'][-1], v_line_obj)
                )

                #averaging values of red, green and blue colors in parking spot image
                #print spotName, np.average(np.mean(parking_spot, 0), 0)
                avg = cv.averageColors(parking_spot)
                spots[spotName] = parking_spot
                colorResult = cv.withinRange(gray_spot_avg, avg, spotName) 

                gray_image = cv.grayImg(parking_spot)
                sharp = cv.sharpen(gray_image)
                edgesResult = cv.cannyedgedetection(sharp,spotName)

                # run other tests here
                maskResult = cv.grayMask(parking_spot)
                # cv2.imshow('image', maskedImg)
                # cv2.waitKey(0)

                vote = 0

                if colorResult == False:
                    vote += 1
                # print vote
                if edgesResult[0] == False:
                    vote += 2
                # print vote
                if maskResult == False:
                    vote += 1
                # print vote


                # if vote > 1:
                if vote == 1:
                    pll.drawBoundBox(parking_spot, red_color)
                    img = pll.boxemup(img, p_lot[-1]['V'][-1], v_line_obj, red_color)	  
                    parkinglotbinaryarray.append(0)				   
                else:
                    pll.drawBoundBox(parking_spot, green_color)
                    img = pll.boxemup(img, p_lot[-1]['V'][-1], v_line_obj, green_color)	
                    parkinglotbinaryarray.append(1); 

                #takes you through each step as it updates
                if len(sys.argv) > 1 and 's' in sys.argv[1]:
                    cv2.imshow('image', img)
                    cv2.waitKey(0)
                    cv2.destroyWindow('image')
                #cv2.imshow("edges", edgesResult[1])

                # Do we need to save these?
                cv.saveImg(parking_spot, spot_dir, spotName)

                #add line object to p_lot
                p_lot[-1]['V'].append(v_line_obj)

    # Print the values to the string to send
    if len(sys.argv) > 1 and 'p' in sys.argv[1]:
        print parkinglotbinaryarray
    if len(sys.argv) > 1 and 'u' in sys.argv[1]:
        serv.sendDataToServer(parkinglotbinaryarray)
    #save the last output
    #cv2.imwrite('out1.jpg', img)
    if len(sys.argv) > 1 and 'f' in sys.argv[1]:
        cv2.imshow('image', img)
        cv2.waitKey(0)
        cv2.destroyWindow('image')
