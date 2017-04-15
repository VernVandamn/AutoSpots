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


import argparse
import os
import numpy as np
import cv2
import csv
import json
import sys
import urllib2
import send_to_server as serv
import platform
from matplotlib import pyplot as plt
# import crfasrnn


spot_dir = 'Spots'
spots = {}
pltimages = []
titles = ['Canny Edge Detection', 'Gray Mask', 'CRF as RNN', 'Darknet']

#class definition for a Point
class Point:
    #constructor
    def __init__(self, point_array):
        self.x = point_array[0]
        self.y = point_array[1]


#class definition for a Line
class Line:
    #constructor
    def __init__(self, point_array1, point_array2):
        self.startPt = Point(point_array1)
        self.endPt = Point(point_array2)
        self.compute_slope()
        self.compute_y_int()

    #method for calculating slope
    def compute_slope(self):
        self.slope = float(self.endPt.y - self.startPt.y) / float(self.endPt.x - self.startPt.x)

    #method for calculating y intercept
    def compute_y_int(self):
        self.yInt = float(self.startPt.y - self.slope*self.startPt.x)

    #method for getting intersection point with another line
    def get_line_intersect(self, line):
        x = float(self.yInt - line.yInt) / float(line.slope - self.slope)
        y = self.slope*x + self.yInt
        pt = Point(x, y)
        return pt

    #method for getting y intersection for some x coordinate
    def get_y_intersect(self, x):
        y = int(round(self.slope*x + self.yInt))
        return y


def average(image, index):
    sum = 0
    count = 0
    for row in image:
        for col in row:
            if col[index] != 0:
                sum += col[index]
                count = count+1
    return float(sum) / count

def averagePng(image):
    count = 0
    for row in image:
        for col in row:
            if col == 255:
                count = count + 1

    return count

def notBlack(image):
    count = 0
    for row in image:
        for col in row:
            if col[0] > 0:
                count = count + 1

    return count

def averageColors(image):
    avg = [average(image, 0), average(image, 1), average(image, 2)]
    return avg

#same image to file
def saveImg(img, dir, name):
    cv2.imwrite(os.path.join(dir, name + ".jpg"), img)


#cuts a parking spot and resizes the result to 1/10 of the original image
def cutParkingSpot(img, point1, point2):
    parkingSpot = img[point1.y:point2.y, point1.x:point2.x]
    width, height = img.shape[:2]
    w,h = parkingSpot.shape[:2]
    resized_img = cv2.resize(parkingSpot, (width/10, height/10))

    return resized_img


#draws line with color between point1 and point2 on img
def draw_line(img, line, color):
    cv2.line(img, (line.startPt.x, line.startPt.y), (line.endPt.x, line.endPt.y), color, 1)


#finds minimum x and y values from end points of 2 lines
def find_min_point(line1, line2):
    min_x = min(line1.startPt.x, line1.endPt.x, line2.startPt.x, line2.endPt.x)
    min_y = min(line1.startPt.y, line1.endPt.y, line2.startPt.y, line2.endPt.y)

    min_point = Point([min_x, min_y])
    return min_point


#finds maximum x and y values from end points of 2 lines
def find_max_point(line1, line2):
    max_x = max(line1.startPt.x, line1.endPt.x, line2.startPt.x, line2.endPt.x)
    max_y = max(line1.startPt.y, line1.endPt.y, line2.startPt.y, line2.endPt.y)

    max_point = Point([max_x, max_y])
    return max_point


#returns a masked image given the original image, and two horizontal lines defining
#the region of interest
def maskImage(img, ver_line1, ver_line2):
    top_left = (ver_line1.startPt.x, ver_line1.startPt.y)
    bot_left = (ver_line1.endPt.x, ver_line1.endPt.y)
    top_right = (ver_line2.startPt.x, ver_line2.startPt.y)
    bot_right = (ver_line2.endPt.x, ver_line2.endPt.y)

    mask = np.zeros(img.shape, dtype=np.uint8)
    roi_corners = np.array([[top_left, top_right, bot_right,bot_left]], dtype=np.int32)  # fill the ROI so it doesn't get wiped out when the mask is applied
    channel_count = img.shape[2]  # i.e. 3 or 4 depending on your image
    ignore_mask_color = (255,)*channel_count
    cv2.fillPoly(mask, roi_corners, ignore_mask_color)
    masked_image = cv2.bitwise_and(img, mask)  # apply the mask
    return(masked_image)

#compares img1 to img2 based on their average values
def withinRange(avg1, avg2, spotName):
    range = 0.2
    blue_diff = abs(avg1[0]-avg2[0])
    green_diff = abs(avg1[1]-avg2[1])
    red_diff = abs(avg1[2]-avg2[2])
    avg_diff = (blue_diff + green_diff + red_diff)/3
    grades = [blue_diff/255.0, green_diff/255.0, red_diff/255.0]
    #print spotName, grades

    for grade in grades:
        if grade > range:
            return False

    #if grades[0] > range or grades[1] > range or green_diff > range:
    #       return False
    return True

    #if blue_diff > range or green_diff > range or red_diff > range:
    #       return False
    #return True

def compareDiffs(avg1, avg2, spotName):
    #AVG as [B, G, R]
    max_diff = 2.0
    fails = 0
    BG1_diff = avg1[0] - avg1[1]
    BR1_diff = avg1[0] - avg1[2]
    GR1_diff = avg1[1] - avg1[2]

    BG2_diff = avg2[0] - avg2[1]
    BR2_diff = avg2[0] - avg2[2]
    GR2_diff = avg2[1] - avg2[2]

    BG_diff = abs(BG1_diff - BG2_diff)
    BR_diff = abs(BR1_diff - BR2_diff)
    GR_diff = abs(GR1_diff - GR2_diff)

    # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
    print avg2
    print spotName, ":", "BG: ", BG_diff/255.0, "BR: ", BR_diff/255.0, "GR: ", GR_diff/255.0, "\n"
    if BG_diff > max_diff:
        fails = fails + 1
    if BR_diff > max_diff:
        fails = fails + 1
    if GR_diff > max_diff:
        fails = fails + 1
    if(fails > 1):
        return False
    return True

#save image from url
def saveImgUrl(url):


    #url = "http://download.thinkbroadband.com/10MB.zip"
    file_name = url.split('/')[-1]
    u = urllib2.urlopen(url)

    infile = open(file_name, 'wb')
    meta = u.info()
    file_size = int(meta.getheaders("Content-Length")[0])
    if len(sys.argv) > 1 and 'p' in sys.argv[1]:
        print "Downloading: %s Bytes: %s" % (file_name, file_size)

    file_size_dl = 0
    block_sz = 8192
    while True:
        buffer = u.read(block_sz)
        if not buffer:
            break

        file_size_dl += len(buffer)
        infile.write(buffer)
        status = r"%10d  [%3.2f%%]" % (file_size_dl, file_size_dl * 100. / file_size)
        status = status + chr(8)*(len(status)+1)
        if len(sys.argv) > 1 and 'p' in sys.argv[1]:
            print status,

    infile.close()

#draw bounding box using lines of specified color around the image
def drawBoundBox(img,  color):
    line_size = 3
    #top horizontal line
    cv2.line(img, (0, 0), (0, len(img[0])), color, line_size)
    #left vertical line
    cv2.line(img, (0, 0), (0, len(img)), color, line_size)
    #bottom horizontal line
    cv2.line(img, (0, len(img)), (len(img[0]), len(img)), color, line_size)
    #right vertical line
    cv2.line(img, (len(img[0]), 0), (len(img[0]), len(img)), color, line_size)

def cannyedgedetection(spotforcanny,parkingspacelocation): #Detects edges
    sigma=0.30
    v = np.median(spotforcanny)
    lower = int(max(0, (1.0 - sigma) * v))
    upper = int(min(255, (1.0 + sigma) * v))
    edges = cv2.Canny(spotforcanny,lower,upper)
    #print edges
    avg = averagePng(edges)
    # if we select print show the edges found in that parking spot
    # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
    print 'Canny results: ', parkingspacelocation, avg
    pltimages.append(edges)
    # cv2.imshow('CV Results', edges)
    # cv2.waitKey(0)

    if avg > 400:
        return False,edges
    return True,edges

def boxemup(image, left, right, color):
    line_sz = 2
    diff = 5

    #top hor
    cv2.line(image, (left.startPt.x+diff, left.startPt.y), (right.startPt.x-diff, right.startPt.y), color, line_sz)
    #bot hor
    cv2.line(image, (left.endPt.x+diff, left.endPt.y), (right.endPt.x-diff, right.endPt.y), color, line_sz)
    #left vert
    cv2.line(image, (left.startPt.x+diff, left.startPt.y), (left.endPt.x+diff, left.endPt.y), color, line_sz)
    #right vert
    cv2.line(image, (right.startPt.x-diff, right.startPt.y), (right.endPt.x-diff, right.endPt.y), color, line_sz)

    return image

def sharpen(spot): #Sharpens the image for better edge detection
    #Create the identity filter, but with the 1 shifted to the right!
    kernel = np.zeros( (9,9), np.float32)
    kernel[4,4] = 2.0   #Identity, times two!
    #Create a box filter:
    boxFilter = np.ones( (9,9), np.float32) / 81.0
    #Subtract the two:
    kernel = kernel - boxFilter
    #Note that we are subject to overflow and underflow here...but I believe that
    # filter2D clips top and bottom ranges on the output, plus you'd need a
    # very bright or very dark pixel surrounded by the opposite type.
    custom = cv2.filter2D(spot, -1, kernel)
    return custom

def grayMask(spot):
    boundries = [
            ([103, 86, 65], [145, 133, 128]),
    ]

    # create NumPy arrays from the boundries
    lower = np.array(boundries[0][0], dtype = "uint8")
    upper = np.array(boundries[0][1], dtype = "uint8")

    # find the colors within the specified boundries and apply
    # the mask
    mask = cv2.inRange(spot, lower, upper)
    output = cv2.bitwise_and(spot, spot, mask = mask)
    total_not_black_pixels = notBlack(output)

    # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
    print 'Gray mask results: ', total_not_black_pixels
    pltimages.append(output)
    # cv2.imshow('CV Results', output)
    # cv2.waitKey(0)


    if total_not_black_pixels > 400:
        return False

    return True

################################################################################
################################################################################
# Running the application
################################################################################
################################################################################

# with open('newParking.json') as data_file:
# with open('Parking-Lot2.json') as data_file:
    # data = json.load(data_file)

black_color = (0,0,0)
red_color = (0,0,255)
green_color = (0,255,0)

#use the images.json for all the images demo is just the noice ones for demo day
ap = argparse.ArgumentParser()
ap.add_argument("-i", "--images", help = "path to the images file")
args = vars(ap.parse_args())
with open(args["images"]) as images_file:
    images = json.load(images_file)

# This is where you specify the number of images to scan
# 0 = only one image
# numImgs = 0
numImgs = images['count']

# clear out spot_results folder
# filelist = [ f for f in os.listdir("../AutospotsCVDisplay/app/assets/images/spots") if f.endswith(".png") ]
# for f in filelist:
    # os.remove('../AutospotsCVDisplay/app/assets/images/spots/' + f)

#loop through all images
for image in images['data']:
    # This is where you specify the number of images to scan
    # 0 = only one image
    # if numImgs > 1:
            # break
    # numImgs = numImgs + 1

    outputDir = image['output']

    img = cv2.imread(image['name'])

    with open(image['json']) as data_file:
        data = json.load(data_file)

    # Create blank file for darknet incase we use it
    blank_darknet = np.zeros(shape=(1,1))
    # Save the image as input.jpg so crfasrnn will read it
    # run crfasrnn before the big loop so we only have to run it once for each picture

    # This will run the machine learning algorithms on the images
    if len(sys.argv) > 1 and 'm' not in sys.argv[1]:
        cv2.imwrite('input.jpg', img)
        height, width, channels = img.shape

        os.system("python crfasrnn.py -g 0")
        # crfasrnn.main('1')
        crfrnn_output = cv2.imread('output.png')
        crfrnn_output = cv2.resize(crfrnn_output, (width, height))

        # darknet machine learning
        # create image to save bounding box locations to
        os.system("./darknet detect cfg/yolo.cfg yolo.weights input.jpg -thresh .11")

        blank_darknet = np.zeros((height,width,3), np.uint8)

        with open('darknet_results.csv', 'r') as f:
            reader = csv.reader(f, delimiter=',')
            for line in reader:
                # line[0]=x1, line[1]=y1, line[2]=x2, line[3]=y2
                cv2.rectangle(blank_darknet,(int(line[0]),int(line[1])),(int(line[2]),int(line[3])),(255,255,255),-1)
        os.remove('darknet_results.csv')


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

        if platform.architecture()[0] == '64bit':
            intLength = 24
        else:
            intLength = 12
        if size == intLength: #check for gray spot
            w,h = img.shape[:2]
            pt = Point(row_data)
            pt1 = Point([pt.x+h/10, pt.y+w/10])

            # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
            print size, h/10, w/10

            gray_spot = cutParkingSpot(img, pt, pt1)
            gray_spot_avg = averageColors(gray_spot)

            # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
            print "Gray Spot", gray_spot_avg

            #cv2.imshow("Grayspot", gray_spot)
            #cv2.waitKey(0)
            # saveImg(gray_spot, spot_dir, "1_Gray_Spot")
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
            h_line_obj = Line(h_line[0], h_line[1]) #get new Line object
            p_lot[-1]['H'].append(h_line_obj) #add new object to p_lot
            #draw_line(img, h_line_obj, black_color)

        top_h_line = p_lot[-1]['H'][0]
        bot_h_line = p_lot[-1]['H'][1]

        #process vertical lines
        for v_line in row_data[1]:
            #create Line object for vertical line
            v_line_obj = Line(
                [v_line[0], top_h_line.get_y_intersect(v_line[0])],
                [v_line[1], bot_h_line.get_y_intersect(v_line[1])]
            )

            #Start extracting spots after first vertical line
            if len(p_lot[-1]['V']) > 0:
                # reset the image list
                pltimages = []
                
                #mask image
                maskedImg = maskImage(img, p_lot[-1]['V'][-1], v_line_obj)

                # cv2.imshow('image', maskedImg)
                # cv2.waitKey(0)

                #get row and column of spot
                spotName = "Row_" + str(len(p_lot)) + "_Col_" + str(len(p_lot[-1]['V']))

                #extract spot from maskedImage and save
                parking_spot = cutParkingSpot(
                        maskedImg,
                        find_min_point(p_lot[-1]['V'][-1], v_line_obj),
                        find_max_point(p_lot[-1]['V'][-1], v_line_obj)
                )

                #averaging values of red, green and blue colors in parking spot image
                #print spotName, np.average(np.mean(parking_spot, 0), 0)

                avg = averageColors(parking_spot)
                spots[spotName] = parking_spot
                colorResult = withinRange(gray_spot_avg, avg, spotName)


                gray_image = cv2.cvtColor(parking_spot, cv2.COLOR_BGR2GRAY)
                sharp = sharpen(gray_image)
                edgesResult = cannyedgedetection(sharp,spotName)

                # run other tests here
                maskResult = grayMask(parking_spot)

                # analyze the crfasrnn output for that spot
                if len(sys.argv) > 1 and 'm' not in sys.argv[1]:
                    # TODO: Masked images not coming out right

                    crfrnn_maskedImg = maskImage(crfrnn_output, p_lot[-1]['V'][-1], v_line_obj)
                    crfrnn_parking_spot = cutParkingSpot(
                            crfrnn_maskedImg,
                            find_min_point(p_lot[-1]['V'][-1], v_line_obj),
                            find_max_point(p_lot[-1]['V'][-1], v_line_obj)
                    )
                    # cv2.imshow('image', blank_darknet)
                    # cv2.waitKey(0)
                    darknet_maskedImg = maskImage(blank_darknet, p_lot[-1]['V'][-1], v_line_obj)
                    darknet_parking_spot = cutParkingSpot(
                            darknet_maskedImg,
                            find_min_point(p_lot[-1]['V'][-1], v_line_obj),
                            find_max_point(p_lot[-1]['V'][-1], v_line_obj)
                    )
                    height, width, channels = crfrnn_parking_spot.shape
                    h = height/2
                    w = width/2
                    # cv2.imshow('image', crfrnn_parking_spot[h-5:h+5, w-5:w+5])
                    # cv2.waitKey(0)
                    machine_learning = False
                    darknet = False
                    # detect in the middle of the spot
                    # detected_pixels = notBlack(crfrnn_parking_spot[h-5:h+5, w-5:w+5])

                    # detect more towards the top of the spot which might be more accurate
                    # because camera angle will make closer things bleed into the spots above them
                    detected_pixels = notBlack(crfrnn_parking_spot[h-10:h, w-5:w+5])
                    if detected_pixels < 70:
                        machine_learning = True


                    detected_darknet_pixels = notBlack(darknet_parking_spot[h-10:h, w-5:w+5])
                    if detected_darknet_pixels < 70:
                        darknet = True

                    # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
                    print 'Detected pixels in center of spot(crfrnn): ', detected_pixels
                    print 'Detected pixels in center of spot(darknet): ', detected_darknet_pixels
                    # cv2.imshow('CV Results', crfrnn_parking_spot)
                    pltimages.append(crfrnn_parking_spot)
                    pltimages.append(darknet_parking_spot)
                    # cv2.waitKey(0)
                    # cv2.destroyWindow('CV Results')

                    for i in range(len(pltimages)):
                        plt.subplot(2,2,i+1),plt.imshow(pltimages[i])
                        plt.title(titles[i])
                        plt.xticks([]),plt.yticks([])

                    plt.savefig(outputDir+'spots/' + spotName)
                    # plt.show()
                
                
                # -- OLD METHOD -- IF ONE IS WRONG GO WITH IT
                # if colorResult == False:
                # drawBoundBox(parking_spot, red_color)
                # img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, red_color)
                # parkinglotbinaryarray.append(0)
                # elif edgesResult[0] == False:
                # drawBoundBox(parking_spot, red_color)
                # img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, red_color)
                # parkinglotbinaryarray.append(0)
                # elif maskResult == False:
                # if maskResult == False:
                # drawBoundBox(parking_spot, red_color)
                # img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, red_color)
                # parkinglotbinaryarray.append(0)
                # else:
                # drawBoundBox(parking_spot, green_color)
                # img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, green_color)
                # parkinglotbinaryarray.append(1);

                vote = 0

                if colorResult == False:
                    vote += 1
                    # print vote
                if edgesResult[0] == False:
                    vote += 2
                    # print vote
                if maskResult == False:
                    vote += 1
                if len(sys.argv) > 1 and 'm' not in sys.argv[1]:
                    if machine_learning == False:
                        vote += 3
                    if darknet == False:
                        vote += 3
                    # print vote


                # if vote > 2:
                if vote > 2:
                # if vote == 1:
                    drawBoundBox(parking_spot, red_color)
                    img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, red_color)
                    parkinglotbinaryarray.append(1)
                else:
                    drawBoundBox(parking_spot, green_color)
                    img = boxemup(img, p_lot[-1]['V'][-1], v_line_obj, green_color)
                    parkinglotbinaryarray.append(0);

                #takes you through each step as it updates
                if len(sys.argv) > 1 and 's' in sys.argv[1]:
                    cv2.imshow('image', img)
                    cv2.waitKey(0)
                    cv2.destroyWindow('image')
                    #cv2.imshow("edges", edgesResult[1])

                # Do we need to save these?
                # saveImg(parking_spot, spot_dir, spotName)

            #add line object to p_lot
            p_lot[-1]['V'].append(v_line_obj)
            #draw_line(img, v_line_obj, black_color)

    # Print the values to the string to send
    # if len(sys.argv) > 1 and 'p' in sys.argv[1]:
    print parkinglotbinaryarray
    # if len(sys.argv) > 1 and 'u' in sys.argv[1]:
    serv.sendDataToServer(parkinglotbinaryarray)
        #save the last output
        #cv2.imwrite('out1.jpg', img)
    # if len(sys.argv) > 1 and 'f' in sys.argv[1]:
    cv2.imwrite(outputDir+'final.png', img)
    cv2.imwrite(outputDir+'output.png', crfrnn_output)
    predict = cv2.imread('predictions.jpg')
    cv2.imwrite(outputDir+'predictions.jpg', predict)
    # cv2.imshow('image', img)
    # cv2.waitKey(0)
    # cv2.destroyWindow('image')
