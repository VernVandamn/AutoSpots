from tkinter import *
from PIL import Image
from PIL import ImageTk
from tkinter import filedialog
import cv2
import numpy as np

app = Tk()

frame=Frame(app,width=1310,height=1000)
frame.grid(row=0,column=0)
canvas=Canvas(frame,bg='#FFFFFF',width=1310,height=1000,scrollregion=(0,0,1300,1000))

vbar=Scrollbar(frame,orient=VERTICAL)
vbar.pack(side=RIGHT,fill=Y)
vbar.config(command=canvas.yview)
canvas.config(yscrollcommand=vbar.set)
canvas.pack(expand=0)

filename = 'dog.jpg'
image = cv2.imread(filename)

image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
edged = cv2.Canny(gray, 50, 100)

boundries = [
    ([103, 86, 65], [145, 133, 128]),
]
# create NumPy arrays from the boundries
lower = np.array(boundries[0][0], dtype = "uint8")
upper = np.array(boundries[0][1], dtype = "uint8")
# find the colors within the specified boundries and apply
# the mask
mask = cv2.inRange(image, lower, upper)
output = cv2.bitwise_and(image, image, mask = mask)

image = Image.fromarray(image)
edged = Image.fromarray(edged)
output = Image.fromarray(output)
image = ImageTk.PhotoImage(image)
edged = ImageTk.PhotoImage(edged)
output = ImageTk.PhotoImage(output)

canvas.create_image(10,10,anchor=NW,image=image)
canvas.create_image(660,10,anchor=NW,image=edged)
canvas.create_image(10,500,anchor=NW,image=output)

app.mainloop()
