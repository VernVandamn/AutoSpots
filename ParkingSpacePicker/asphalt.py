import numpy as np
import argparse
import cv2

# construct the argument parse and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-i", "--image", help = "path to the image")
args = vars(ap.parse_args())

# load the image
image = cv2.imread(args["image"])

# define the list of boundries
# BGR values for top and bottom boundries for colors
# red, blue, yellow, gray, in that order
boundries = [
    ([17, 15,100], [50, 56, 200]),
    ([86, 31, 4], [220, 88, 50]),
    ([25, 146, 190], [62, 174, 250]),
    ([103, 86, 65], [145, 133, 128]),
    ([195, 195, 195], [205, 205, 205]),
    ([100, 100, 100], [130, 130, 130])
]

# loop over the boundries
for (lower, upper) in boundries:
    # create NumPy arrays from the boundries
    lower = np.array(lower, dtype = "uint8")
    upper = np.array(upper, dtype = "uint8")

    # find the colors within the specified boundries and apply
    # the mask
    mask = cv2.inRange(image, lower, upper)
    output = cv2.bitwise_and(image, image, mask = mask)
    
    cv2.imshow("images", np.hstack([image, output]))
    cv2.waitKey(0)
