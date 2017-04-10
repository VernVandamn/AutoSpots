var canvas = this.__canvas = new fabric.Canvas('c', { selection: false });
fabric.Object.prototype.originX = fabric.Object.prototype.originY = 'center';

S1 = new Strip();
//S1.addStrip();

function makeLine(coords) {
  return new fabric.Line(coords, {
    fill: 'red',
    stroke: 'red',
    strokeWidth: 5,
    selectable: false
  });
}
  
function makeCircle(left, top, line1, line2, line3, line4) {
  var c = new fabric.Circle({
    left: left,
    top: top,
    strokeWidth: 5,
    radius: 12,
    fill: '#fff',
    stroke: '#666'
  });
  c.hasControls = c.hasBorders = false;

  c.line1 = line1;
  c.line2 = line2;
  c.line3 = line3;
  c.line4 = line4;

  return c;
}

function Strip() {
  this.top = makeLine([ 100, 175, 350, 175 ]), //top
  this.right = makeLine([ 350, 175, 350, 350 ]), // right
  this.bot = makeLine([ 350, 350, 100, 350 ]), //bot
  this.left = makeLine([ 100, 350, 100, 175 ]); //left
  this.addStrip = function(){
    // x1, y1, x2,  y2
    //var line = makeLine([ 250, 125, 250, 175 ]), //neck
    //newCoords = [ top.get('x1')+50, 175, left.get('x1')+50, 350 ];
    //console.log(newCoords);
    //var mid = makeLine(newCoords);
    //console.log(mid);
    canvas.add(top, right, bot, left);
    //canvas.add(mid);
    canvas.add(
      //makeCircle(line.get('x1'), line.get('y1'), null, line),
      makeCircle(left.get('x2'), left.get('y2'), left, top), //good
      makeCircle(top.get('x2'), top.get('y2'), top, right), // 
      makeCircle(right.get('x2'), right.get('y2'), right, bot), // good
      makeCircle(left.get('x1'), left.get('y1'), bot, left)  // good
      //makeCircle(left.get('x2'), left.get('y2'), left)
    );
  }
}

canvas.on('object:moving', function(e) {
    var p = e.target;
    p.line1 && p.line1.set({ 'x2': p.left, 'y2': p.top });
    p.line2 && p.line2.set({ 'x1': p.left, 'y1': p.top });
    p.line3 && p.line3.set({ 'x1': p.left, 'y1': p.top });
    p.line4 && p.line4.set({ 'x1': p.left, 'y1': p.top });
    canvas.renderAll();
  });  