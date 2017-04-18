import argparse as ap

def main():
  parser = ap.ArgumentParser(description="My Script")
  parser.add_argument("--myArg")
  args, leftovers = parser.parse_known_args()

  if args.myArg is not None:
    print "myArg has been set (value is %s)" % args.myArg
  else:
  	print 'nada'

if __name__ == "__main__":
	main()