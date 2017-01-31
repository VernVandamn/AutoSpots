import numpy as np
import cv2

im = cv2.imread('car2.jpg')
imgray = cv2.cvtColor(im, cv2.COLOR_BGR2GRAY)
ret, thresh = cv2.threshold(imgray, 127, 255, 0)
im2, contours, hierarchy = cv2.findContours(thresh, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

im3 = cv2.drawContours(im, contours, -1, (0,255,0), 1)
# cnt = contours[4]
# im3 = cv2.drawContours(im, [cnt], 0, (0,255,0), 1)
cv2.imshow('image', im3)
cv2.waitKey(0)
