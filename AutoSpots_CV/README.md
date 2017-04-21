# Autospots Computer Vision

## Setup
The computer vision works off of OpenCV and 2 other machine learning object detection
algorithms. To run the code here you need to install the code for both the machine
learning algorithms.

First there is [crfasrnn](https://github.com/torrvision/crfasrnn) this is the
algorithm that does the semantic image segmentation. The installation instructions
for their algorithm is located in the link above. Once it is installed and you have
run the python demo they provide you need to move all the files starting with TVG
to this folder as well as copying the caffe folder you set up into here.

The second machine learning algorithm we use is Darknet's YOLO. The setup code can
be found [here](https://pjreddie.com/darknet/install/). This does need to be installed
with OpenCV so the images can be output to the folders. The installation instructions
for OpenCV can be found [here](http://docs.opencv.org/2.4.13.2/doc/tutorials/introduction/linux_install/linux_install.html#linux-installation). Darknet is set up to work with OpenCV versions less than 3.0. Installing
2.4.13 should work great. If you don't have CUDA capability make sure that you set
CUDA = 0 in the darknet makefile. Once you have darknet running the data folder,
cfg folder and the yolo.weights you downloaded need to be moved into the AutoSpots_CV
folder here.

The following python packages are needed for the python scripts to run. All these
can be installed with the python package manager pip.

- numpy
- cv2
- urllib2
- matplotlib
- requests
- cloudinary

Some of the above packages might already be included depending if you are using
anaconda python or not. if you attempt installation with pip though it will get the
ones you need

After installing those 2 machine learning algorithms you should be all set to
get the scripts running below.

## Running the computer vision backend
To turn on the computer vision back end to Autospots call the script below.
```
python allspark.py
```
Adding a -f option will force a single update to the server.

The allspark script calls Autospots.py which is the script that analyzes the parking
lot and calls the machine learning algorithms on it.

## How it works

This code analyzes a parking lot and finds the empty parking spaces. The base code is built off of opencv and a project [Parking-Lot-Occupancy-Tracking](https://github.com/rugbyprof/Parking-Lot-Occupancy-Tracking). We have imporoved the methods they used with some techniques of our own as well as 2 different machine learning algorithms.

## Machine learning

We use semantic image segmentation done with Conditional Random Fields as Recurrent Neural Networks. More information on this can be found on the project's [github](https://github.com/torrvision/crfasrnn) or their [demo website](http://crfasrnn.torr.vision)

We also use darknet, an open source neural network framework. They have implemented a object detection algorithm yolo that applies a neural network to the entire image and works from there finding objects faster than Fast-RCNN. Here is the links for their [github](https://github.com/pjreddie/darknet) and [website](https://pjreddie.com/darknet/)
