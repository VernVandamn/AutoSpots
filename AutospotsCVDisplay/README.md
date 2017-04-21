# Autospots Website

To run the code here the binary files are needed for a default rails app. If you
are going to run the app you should have ruby on rails installed already if you
have a existing rails app you can copy the bin folder into this folder or create
a new rails app with:
```
rails g new TestWebsite
```
Then copy the bin folder into this one and delete the folder for the app you just
created.

After acquiring the bin files just run the following scripts and you should get
a server on the local host.
```
bundle install
rails db:migrate
rails s
```
If you get an error mentioning puma. you need to comment out the last few lines of
./config/puma.rb. Amazon Web Services needs those lines there to run but sometimes
they can cause errors on a personal system.
