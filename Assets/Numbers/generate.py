from PIL import Image, ImageDraw, ImageFont

img = Image.new('RGBA', (500 * 10, 500), color = (0,0,0,0))
d = ImageDraw.Draw(img)
ft = ImageFont.truetype('Courier Prime Bold.ttf', 300)

for i in range(10):
    print(i)
    size = d.textsize(str(i), font=ft)
    d.text(((500*i)+(500/2)-size[0]/2, (500/2)-size[1]/2), str(i), fill=(0,0,0,255), font=ft)

img.save("numbers.png")
