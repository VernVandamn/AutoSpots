import cv2
import numpy as np

def grayImg(parking_spot):
    return cv2.cvtColor(parking_spot, cv2.COLOR_BGR2GRAY)

#same image to file
def saveImg(img, dir, name):
    cv2.imwrite(os.path.join(dir, name + ".jpg"), img)


def average(image, index):
    sum = 0
    count = 0
        for row in image:
            for col in row:
                if col[index] != 0:
                    sum += col[index]
                    count = count+1
        return float(sum) / count

def averageColors(image):
    avg = [average(image, 0), average(image, 1), average(image, 2)]
    return avg

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
    #	return False
    return True

    #if blue_diff > range or green_diff > range or red_diff > range:
    #	return False
    #return True

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
    print total_not_black_pixels
    # cv2.imshow('image', output)
    # cv2.waitKey(0)


    if total_not_black_pixels > 400:
        return False

    return True
def notBlack(image):
    count = 0
    for row in image:
        for col in row:
            if col[0] > 0:
                count = count + 1

    return count

def cannyedgedetection(spotforcanny,parkingspacelocation): #Detects edges
    sigma=0.30
    v = np.median(spotforcanny)
    lower = int(max(0, (1.0 - sigma) * v))
    upper = int(min(255, (1.0 + sigma) * v))
    edges = cv2.Canny(spotforcanny,lower,upper)
    #print edges
    avg = averagePng(edges)
    if len(sys.argv) > 1 and 'p' in sys.argv[1]:
        print parkingspacelocation, avg

    if avg > 400:
        return False,edges
    return True,edges

def averagePng(image):
    count = 0
    for row in image:
        for col in row:
            if col == 255:
                count = count + 1

    return count

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
