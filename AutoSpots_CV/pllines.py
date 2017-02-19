import cv2

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

#cuts a parking spot and resizes the result to 1/10 of the original image
def cutParkingSpot(img, point1, point2):
    parkingSpot = img[point1.y:point2.y, point1.x:point2.x]
    width, height = img.shape[:2]
    w,h = parkingSpot.shape[:2]
    resized_img = cv2.resize(parkingSpot, (width/10, height/10))

    return resized_img

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
