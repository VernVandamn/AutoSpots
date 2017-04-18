import requests
import json
import cloudinary
import argparse
from cloudinary import uploader as up
import os, shutil
import sched, time

cloudinary.config(
	cloud_name= "ddzdneuxe",
	api_key= "555971697581325",
	api_secret= "6PjnLcArSUuziXJyz65DyDnWzls"
)

output_base = './output/'
jsonInput = { 'data': [] };

if __name__ == "__main__":
  main()

def main():
	# Set up to force an update with a input argument
	# construct the argument parse and parse the arguments
	ap = argparse.ArgumentParser()
	ap.add_argument("-f", "--force", help = "Force update to server")
	args = vars(ap.parse_args())

	# load the image
	if args.force is not None:
		getParkingInfo()
		runCV()
		uploadResults()
	else:
		#######################################################################
		# Scheduler
		#######################################################################
		s = sched.scheduler(time.time, time.sleep)
		def do_something(sc): 
	    # print "Doing stuff..."
			getParkingInfo()
			runCV()
			uploadResults()
			print jsonInput
	    s.enter(300, 1, do_something)

		s.enter(300, 1, do_something)
		s.run()


#######################################################################
# Functions
#######################################################################
def getParkingInfo():
	# This will return a json object of all the parking spaces set up on the website
	parkingSpaces = requests.get('http://www.autospots.org/parking_spaces.json').json()

	jsonInput['count'] = len(parkingSpaces)

	# Do the following for every set up parking lot
	for space in parkingSpaces:
		pid = str(space['id'])
		# print pid
		coords = json.loads(space['parking_coords'])
		imgUrl = str(space['image_url'])

		# First create the folders in the output folder that the images will go into
		picDir = output_base + pid + '/'
		spotsDir = picDir	+ 'spots/'
		if not os.path.exists(output_base):
			os.mkdir(output_base)
		if not os.path.exists(picDir):
			os.mkdir(picDir)
		if not os.path.exists(spotsDir):
			os.mkdir(spotsDir)

		# Download the referenced image into the new created folder
		imgLoc = picDir+'orig.jpg'
		with open(imgLoc, 'wb') as pic:
			# print 'imgUrl:', imgUrl
			r = requests.get(imgUrl, stream=True)

			if not r.ok:
				print r

			for block in r.iter_content(512):
				if not block:
					break
				pic.write(block)
			# pic.write(r.content)

		# Create the json file from the selecting parking spots we downloaded
		jsonLoc = picDir+'parkingSpots.json'
		with open(jsonLoc, 'wb') as jsonFile:
			json.dump(coords, jsonFile)

		# Then we need to add the info the the inner object
		spaceOutput = { 
			'output':	picDir,
			'json': 	jsonLoc,
			'name': 	imgLoc,
			'id':			pid
		}
		jsonInput['data'].append(spaceOutput)

	# save the object we created as a json file for the cv part to read
	with open('images.json', 'w') as fp:
		json.dump(jsonInput, fp)

def runCV():
	os.system("python ./AutoSpots.py -i images.json")
	print 'Parking has been analyzed'

def cleanup():
	shutil.rmtree('./output/')

def uploadResults():
	print 'Uploading images to server'
	data = jsonInput['data']
	for space in data:
		# Upload final image
		respose = up.upload(
			space['output']+'final.png', 
			tags=space['id']+'final',
			folder=space['id'],
			public_id='final'
		)
		# Upload crfrnn image
		respose = up.upload(
			space['output']+'output.png', 
			folder=space['id'],
			public_id='output'
		)
		# Upload darknet image
		respose = up.upload(
			space['output']+'predictions.jpg', 
			folder=space['id'],
			public_id='predictions'
		)
		spotsDir = space['output']+'spots/'
		for spot in os.listdir(output_base+space['id']+'/spots'):
			respose = up.upload(
				spotsDir+spot, 
				tags=space['id']+'spot',
				folder=space['id']+'/spots',
				public_id=spot[:-4]
			)

