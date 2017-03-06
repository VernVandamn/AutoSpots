import numpy as np
import cv2

# Create a black image
img = np.zeros((512,512,3), np.uint8)
cv2.rectangle(img,(384,0),(510,128),(255,255,255),-1)

cv2.imshow('img', img)
cv2.waitKey(0)