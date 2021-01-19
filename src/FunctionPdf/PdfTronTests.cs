using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using pdftron.Common;
using pdftron.PDF;
using pdftron.SDF;
using System.Web.Http;
using pdftron.Filters;

namespace FunctionPdf
{
    public static class PdfTronTests
    {
		[FunctionName("AddImageTest")]
		public static IActionResult AddImageTest(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
			ExecutionContext context,
			ILogger log)
		{
			// Relative path to the folder containing test files.
			string input_path = context.FunctionAppDirectory + "\\TestFiles\\";
			log.LogInformation(input_path);

			try
			{
				using (PDFDoc doc = new PDFDoc())
				using (ElementBuilder bld = new ElementBuilder())   // Used to build new Element objects
				using (ElementWriter writer = new ElementWriter())  // Used to write Elements to the page	
				{
					Page page = doc.PageCreate();   // Start a new page 
					writer.Begin(page);             // Begin writing to this page

					// ----------------------------------------------------------
					// Embed a JPEG image to the output document. 
					Image img = Image.Create(doc, input_path + "peppers.jpg");

					// You can also directly add any .NET Bitmap. The following commented-out code 
					// is equivalent to the above line:
					//    System.Drawing.Bitmap bmp;
					//    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(input_path + "peppers.jpg");
					//    Image img = Image.Create(doc, bmp);

					Element element = bld.CreateImage(img, 50, 500, img.GetImageWidth() / 2, img.GetImageHeight() / 2);
					writer.WritePlacedElement(element);

					// ----------------------------------------------------------
					// Add a PNG image to the output file
					img = Image.Create(doc, input_path + "butterfly.png");
					element = bld.CreateImage(img, new Matrix2D(100, 0, 0, 100, 300, 500));
					writer.WritePlacedElement(element);

					// ----------------------------------------------------------
					// Add a GIF image to the output file
					img = Image.Create(doc, input_path + "pdfnet.gif");
					element = bld.CreateImage(img, new Matrix2D(img.GetImageWidth(), 0, 0, img.GetImageHeight(), 50, 350));
					writer.WritePlacedElement(element);

					// ----------------------------------------------------------
					// Add a TIFF image to the output file
					img = Image.Create(doc, input_path + "grayscale.tif");
					element = bld.CreateImage(img, new Matrix2D(img.GetImageWidth(), 0, 0, img.GetImageHeight(), 10, 50));
					writer.WritePlacedElement(element);

					writer.End();           // Save the page
					doc.PagePushBack(page); // Add the page to the document page sequence

					// ----------------------------------------------------------
					// Add a BMP image to the output file
					/*
                    bmp = new System.Drawing.Bitmap(input_path + "pdftron.bmp");
					img = Image.Create(doc, bmp);
					element = bld.CreateImage(img, new Matrix2D(bmp.Width, 0, 0, bmp.Height, 255, 700));
					writer.WritePlacedElement(element);
			
					writer.End();	// Finish writing to the page
					doc.PagePushBack(page);
                    */

					// ----------------------------------------------------------
					// Embed a monochrome TIFF. Compress the image using lossy JBIG2 filter.

					page = doc.PageCreate(new pdftron.PDF.Rect(0, 0, 612, 794));
					writer.Begin(page); // begin writing to this page

					// Note: encoder hints can be used to select between different compression methods. 
					// For example to instruct PDFNet to compress a monochrome image using JBIG2 compression.
					ObjSet hint_set = new ObjSet();
					Obj enc = hint_set.CreateArray();  // Initialize encoder 'hint' parameter 
					enc.PushBackName("JBIG2");
					enc.PushBackName("Lossy");

					img = pdftron.PDF.Image.Create(doc, input_path + "multipage.tif", enc);
					element = bld.CreateImage(img, new Matrix2D(612, 0, 0, 794, 0, 0));
					writer.WritePlacedElement(element);

					writer.End();           // Save the page
					doc.PagePushBack(page); // Add the page to the document page sequence

					// ----------------------------------------------------------
					// Add a JPEG2000 (JP2) image to the output file

					// Create a new page 
					page = doc.PageCreate();
					writer.Begin(page); // Begin writing to the page

					// Embed the image.
					img = pdftron.PDF.Image.Create(doc, input_path + "palm.jp2");

					// Position the image on the page.
					element = bld.CreateImage(img, new Matrix2D(img.GetImageWidth(), 0, 0, img.GetImageHeight(), 96, 80));
					writer.WritePlacedElement(element);

					// Write 'JPEG2000 Sample' text string under the image.
					writer.WriteElement(bld.CreateTextBegin(pdftron.PDF.Font.Create(doc, pdftron.PDF.Font.StandardType1Font.e_times_roman), 32));
					element = bld.CreateTextRun("JPEG2000 Sample");
					element.SetTextMatrix(1, 0, 0, 1, 190, 30);
					writer.WriteElement(element);
					writer.WriteElement(bld.CreateTextEnd());

					writer.End();   // Finish writing to the page
					doc.PagePushBack(page);

					// Save as a linearized file which is most popular  
					// and effective format for quick PDF Viewing.
					var bytes = doc.Save(SDFDoc.SaveOptions.e_linearized);
					return new FileContentResult(bytes, "application/pdf");
				}
			}
			catch (PDFNetException e)
			{
				log.LogInformation(e.Message);
				return new InternalServerErrorResult();
			}
		}

		[FunctionName("ElementBuilderTest")]
        public static IActionResult ElementBuilderTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            // Relative path to the folder containing test files.
            string input_path = context.FunctionAppDirectory + "\\TestFiles\\";
            log.LogInformation(input_path);

			try
			{
				using (PDFDoc doc = new PDFDoc())
				using (ElementBuilder eb = new ElementBuilder())        // ElementBuilder is used to build new Element objects
				using (ElementWriter writer = new ElementWriter())  // ElementWriter is used to write Elements to the page	
				{
					// Start a new page ------------------------------------
					// Position an image stream on several places on the page
					Page page = doc.PageCreate(new Rect(0, 0, 612, 794));

					writer.Begin(page); // begin writing to this page

					// Create an Image that can be reused multiple times in the document or 
					// multiple on the same page.
					MappedFile img_file = new MappedFile(input_path + "peppers.jpg");
					FilterReader img_data = new FilterReader(img_file);
					Image img = Image.Create(doc, img_data, 400, 600, 8, ColorSpace.CreateDeviceRGB(), Image.InputFilter.e_jpeg);

					Element element = eb.CreateImage(img, new Matrix2D(200, -145, 20, 300, 200, 150));
					writer.WritePlacedElement(element);

					GState gstate = element.GetGState();    // use the same image (just change its matrix)
					gstate.SetTransform(200, 0, 0, 300, 50, 450);
					writer.WritePlacedElement(element);

					// use the same image again (just change its matrix).
					writer.WritePlacedElement(eb.CreateImage(img, 300, 600, 200, -150));

					writer.End();  // save changes to the current page
					doc.PagePushBack(page);

					// Start a new page ------------------------------------
					// Construct and draw a path object using different styles
					page = doc.PageCreate(new Rect(0, 0, 612, 794));

					writer.Begin(page); // begin writing to this page
					eb.Reset();         // Reset GState to default


					eb.PathBegin();     // start constructing the path				                            
					eb.MoveTo(306, 396);
					eb.CurveTo(681, 771, 399.75, 864.75, 306, 771);
					eb.CurveTo(212.25, 864.75, -69, 771, 306, 396);
					eb.ClosePath();
					element = eb.PathEnd();         // the path is now finished
					element.SetPathFill(true);      // the path should be filled

					// Set the path color space and color
					gstate = element.GetGState();
					gstate.SetFillColorSpace(ColorSpace.CreateDeviceCMYK());
					gstate.SetFillColor(new ColorPt(1, 0, 0, 0));  // cyan
					gstate.SetTransform(0.5, 0, 0, 0.5, -20, 300);
					writer.WritePlacedElement(element);

					// Draw the same path using a different stroke color
					element.SetPathStroke(true);        // this path is should be filled and stroked
					gstate.SetFillColor(new ColorPt(0, 0, 1, 0));  // yellow
					gstate.SetStrokeColorSpace(ColorSpace.CreateDeviceRGB());
					gstate.SetStrokeColor(new ColorPt(1, 0, 0));  // red
					gstate.SetTransform(0.5, 0, 0, 0.5, 280, 300);
					gstate.SetLineWidth(20);
					writer.WritePlacedElement(element);

					// Draw the same path with with a given dash pattern
					element.SetPathFill(false); // this path is should be only stroked
					gstate.SetStrokeColor(new ColorPt(0, 0, 1));  // blue
					gstate.SetTransform(0.5, 0, 0, 0.5, 280, 0);
					double[] dash_pattern = { 30 };
					gstate.SetDashPattern(dash_pattern, 0);
					writer.WritePlacedElement(element);

					// Use the path as a clipping path
					writer.WriteElement(eb.CreateGroupBegin()); // Save the graphics state
																// Start constructing a new path (the old path was lost when we created 
																// a new Element using CreateGroupBegin()).
					eb.PathBegin();
					eb.MoveTo(306, 396);
					eb.CurveTo(681, 771, 399.75, 864.75, 306, 771);
					eb.CurveTo(212.25, 864.75, -69, 771, 306, 396);
					eb.ClosePath();
					element = eb.PathEnd(); // path is now built
					element.SetPathClip(true);  // this path is a clipping path
					element.SetPathStroke(true);        // this path is should be filled and stroked
					gstate = element.GetGState();
					gstate.SetTransform(0.5, 0, 0, 0.5, -20, 0);
					writer.WriteElement(element);
					writer.WriteElement(eb.CreateImage(img, 100, 300, 400, 600));
					writer.WriteElement(eb.CreateGroupEnd());   // Restore the graphics state

					writer.End();  // save changes to the current page
					doc.PagePushBack(page);


					// Start a new page ------------------------------------
					page = doc.PageCreate(new Rect(0, 0, 612, 794));

					writer.Begin(page); // begin writing to this page
					eb.Reset();         // Reset GState to default

					// Begin writing a block of text
					element = eb.CreateTextBegin(Font.Create(doc, Font.StandardType1Font.e_times_roman), 12);
					writer.WriteElement(element);

					string data = "Hello World!";
					element = eb.CreateTextRun(data);
					element.SetTextMatrix(10, 0, 0, 10, 0, 600);
					element.GetGState().SetLeading(15);      // Set the spacing between lines
					writer.WriteElement(element);

					writer.WriteElement(eb.CreateTextNewLine());  // New line

					element = eb.CreateTextRun(data);
					gstate = element.GetGState();
					gstate.SetTextRenderMode(GState.TextRenderingMode.e_stroke_text);
					gstate.SetCharSpacing(-1.25);
					gstate.SetWordSpacing(-1.25);
					writer.WriteElement(element);

					writer.WriteElement(eb.CreateTextNewLine());  // New line

					element = eb.CreateTextRun(data);
					gstate = element.GetGState();
					gstate.SetCharSpacing(0);
					gstate.SetWordSpacing(0);
					gstate.SetLineWidth(3);
					gstate.SetTextRenderMode(GState.TextRenderingMode.e_fill_stroke_text);
					gstate.SetStrokeColorSpace(ColorSpace.CreateDeviceRGB());
					gstate.SetStrokeColor(new ColorPt(1, 0, 0));    // red
					gstate.SetFillColorSpace(ColorSpace.CreateDeviceCMYK());
					gstate.SetFillColor(new ColorPt(1, 0, 0, 0));   // cyan
					writer.WriteElement(element);

					writer.WriteElement(eb.CreateTextNewLine());  // New line

					// Set text as a clipping path to the image.
					element = eb.CreateTextRun(data);
					gstate = element.GetGState();
					gstate.SetTextRenderMode(GState.TextRenderingMode.e_clip_text);
					writer.WriteElement(element);

					// Finish the block of text
					writer.WriteElement(eb.CreateTextEnd());

					// Draw an image that will be clipped by the above text
					writer.WriteElement(eb.CreateImage(img, 10, 100, 1300, 720));

					writer.End();  // save changes to the current page
					doc.PagePushBack(page);

					// Start a new page ------------------------------------
					//
					// The example illustrates how to embed the external font in a PDF document. 
					// The example also shows how ElementReader can be used to copy and modify 
					// Elements between pages.

					using (ElementReader reader = new ElementReader())
					{
						// Start reading Elements from the last page. We will copy all Elements to 
						// a new page but will modify the font associated with text.
						reader.Begin(doc.GetPage(doc.GetPageCount()));

						page = doc.PageCreate(new Rect(0, 0, 1300, 794));

						writer.Begin(page); // begin writing to this page
						eb.Reset();         // Reset GState to default

						// Embed an external font in the document.
						Font font = Font.CreateTrueTypeFont(doc, input_path + "font.ttf");

						while ((element = reader.Next()) != null)   // Read page contents
						{
							if (element.GetType() == Element.Type.e_text)
							{
								element.GetGState().SetFont(font, 12);
							}

							writer.WriteElement(element);
						}

						reader.End();
						writer.End();  // save changes to the current page

						doc.PagePushBack(page);


						// Start a new page ------------------------------------
						//
						// The example illustrates how to embed the external font in a PDF document. 
						// The example also shows how ElementReader can be used to copy and modify 
						// Elements between pages.

						// Start reading Elements from the last page. We will copy all Elements to 
						// a new page but will modify the font associated with text.
						reader.Begin(doc.GetPage(doc.GetPageCount()));

						page = doc.PageCreate(new Rect(0, 0, 1300, 794));

						writer.Begin(page); // begin writing to this page
						eb.Reset();         // Reset GState to default

						// Embed an external font in the document.
						Font font2 = Font.CreateType1Font(doc, input_path + "Misc-Fixed.pfa");

						while ((element = reader.Next()) != null)   // Read page contents
						{
							if (element.GetType() == Element.Type.e_text)
							{
								element.GetGState().SetFont(font2, 12);
							}

							writer.WriteElement(element);
						}

						reader.End();
						writer.End();  // save changes to the current page
						doc.PagePushBack(page);


						// Start a new page ------------------------------------
						page = doc.PageCreate();
						writer.Begin(page); // begin writing to this page
						eb.Reset();         // Reset GState to default

						// Begin writing a block of text
						element = eb.CreateTextBegin(Font.Create(doc, Font.StandardType1Font.e_times_roman), 12);
						element.SetTextMatrix(1.5, 0, 0, 1.5, 50, 600);
						element.GetGState().SetLeading(15); // Set the spacing between lines
						writer.WriteElement(element);

						string para = "A PDF text object consists of operators that can show " +
						"text strings, move the text position, and set text state and certain " +
						"other parameters. In addition, there are three parameters that are " +
						"defined only within a text object and do not persist from one text " +
						"object to the next: Tm, the text matrix, Tlm, the text line matrix, " +
						"Trm, the text rendering matrix, actually just an intermediate result " +
						"that combines the effects of text state parameters, the text matrix " +
						"(Tm), and the current transformation matrix";

						int para_end = para.Length;
						int text_run = 0;
						int text_run_end;

						double para_width = 300; // paragraph width is 300 units
						double cur_width = 0;

						while (text_run < para_end)
						{
							text_run_end = para.IndexOf(' ', text_run);
							if (text_run_end < 0)
								text_run_end = para_end - 1;

							string text = para.Substring(text_run, text_run_end - text_run + 1);
							element = eb.CreateTextRun(text);
							if (cur_width + element.GetTextLength() < para_width)
							{
								writer.WriteElement(element);
								cur_width += element.GetTextLength();
							}
							else
							{
								writer.WriteElement(eb.CreateTextNewLine());  // New line
								text = para.Substring(text_run, text_run_end - text_run + 1);
								element = eb.CreateTextRun(text);
								cur_width = element.GetTextLength();
								writer.WriteElement(element);
							}

							text_run = text_run_end + 1;
						}

						// -----------------------------------------------------------------------
						// The following code snippet illustrates how to adjust spacing between 
						// characters (text runs).
						element = eb.CreateTextNewLine();
						writer.WriteElement(element);  // Skip 2 lines
						writer.WriteElement(element);

						writer.WriteElement(eb.CreateTextRun("An example of space adjustments between inter-characters:"));
						writer.WriteElement(eb.CreateTextNewLine());

						// Write string "AWAY" without space adjustments between characters.
						element = eb.CreateTextRun("AWAY");
						writer.WriteElement(element);

						writer.WriteElement(eb.CreateTextNewLine());

						// Write string "AWAY" with space adjustments between characters.
						element = eb.CreateTextRun("A");
						writer.WriteElement(element);

						element = eb.CreateTextRun("W");
						element.SetPosAdjustment(140);
						writer.WriteElement(element);

						element = eb.CreateTextRun("A");
						element.SetPosAdjustment(140);
						writer.WriteElement(element);

						element = eb.CreateTextRun("Y again");
						element.SetPosAdjustment(115);
						writer.WriteElement(element);

						// Draw the same strings using direct content output...
						writer.Flush();  // flush pending Element writing operations.

						// You can also write page content directly to the content stream using 
						// ElementWriter.WriteString(...) and ElementWriter.WriteBuffer(...) methods.
						// Note that if you are planning to use these functions you need to be familiar
						// with PDF page content operators (see Appendix A in PDF Reference Manual). 
						// Because it is easy to make mistakes during direct output we recommend that 
						// you use ElementBuilder and Element interface instead.
						writer.WriteString("T* T* "); // New Lines 
													  // writer.WriteElement(eb.CreateTextNewLine()); 
						writer.WriteString("(Direct output to PDF page content stream:) Tj  T* ");
						writer.WriteString("(AWAY) Tj T* ");
						writer.WriteString("[(A)140(W)140(A)115(Y again)] TJ ");

						// Finish the block of text
						writer.WriteElement(eb.CreateTextEnd());

						writer.End();  // save changes to the current page
						doc.PagePushBack(page);

						// Start a new page ------------------------------------

						// Image Masks
						//
						// In the opaque imaging model, images mark all areas they occupy on the page as 
						// if with opaque paint. All portions of the image, whether black, white, gray, 
						// or color, completely obscure any marks that may previously have existed in the 
						// same place on the page.
						// In the graphic arts industry and page layout applications, however, it is common 
						// to crop or 'mask out' the background of an image and then place the masked image 
						// on a different background, allowing the existing background to show through the 
						// masked areas. This sample illustrates how to use image masks. 

						page = doc.PageCreate();
						writer.Begin(page); // begin writing to the page

						// Create the Image Mask
						MappedFile imgf = new MappedFile(input_path + "imagemask.dat");
						FilterReader mask_read = new FilterReader(imgf);

						ColorSpace device_gray = ColorSpace.CreateDeviceGray();
						Image mask = Image.Create(doc, mask_read, 64, 64, 1, device_gray, Image.InputFilter.e_ascii_hex);

						mask.GetSDFObj().PutBool("ImageMask", true);

						element = eb.CreateRect(0, 0, 612, 794);
						element.SetPathStroke(false);
						element.SetPathFill(true);
						element.GetGState().SetFillColorSpace(device_gray);
						element.GetGState().SetFillColor(new ColorPt(0.8));
						writer.WritePlacedElement(element);

						element = eb.CreateImage(mask, new Matrix2D(200, 0, 0, -200, 40, 680));
						element.GetGState().SetFillColor(new ColorPt(0.1));
						writer.WritePlacedElement(element);

						element.GetGState().SetFillColorSpace(ColorSpace.CreateDeviceRGB());
						element.GetGState().SetFillColor(new ColorPt(1, 0, 0));
						element = eb.CreateImage(mask, new Matrix2D(200, 0, 0, -200, 320, 680));
						writer.WritePlacedElement(element);

						element.GetGState().SetFillColor(new ColorPt(0, 1, 0));
						element = eb.CreateImage(mask, new Matrix2D(200, 0, 0, -200, 40, 380));
						writer.WritePlacedElement(element);

						{
							// This sample illustrates Explicit Masking. 
							img = Image.Create(doc, input_path + "peppers.jpg");

							// mask is the explicit mask for the primary (base) image
							img.SetMask(mask);

							element = eb.CreateImage(img, new Matrix2D(200, 0, 0, -200, 320, 380));
							writer.WritePlacedElement(element);
						}

						writer.End();  // save changes to the current page
						doc.PagePushBack(page);

						// Transparency sample ----------------------------------

						// Start a new page -------------------------------------
						page = doc.PageCreate();
						writer.Begin(page); // begin writing to this page
						eb.Reset();         // Reset the GState to default

						// Write some transparent text at the bottom of the page.
						element = eb.CreateTextBegin(Font.Create(doc, Font.StandardType1Font.e_times_roman), 100);

						// Set the text knockout attribute. Text knockout must be set outside of 
						// the text group.
						gstate = element.GetGState();
						gstate.SetTextKnockout(false);
						gstate.SetBlendMode(GState.BlendMode.e_bl_difference);
						writer.WriteElement(element);

						element = eb.CreateTextRun("Transparency");
						element.SetTextMatrix(1, 0, 0, 1, 30, 30);
						gstate = element.GetGState();
						gstate.SetFillColorSpace(ColorSpace.CreateDeviceCMYK());
						gstate.SetFillColor(new ColorPt(1, 0, 0, 0));

						gstate.SetFillOpacity(0.5);
						writer.WriteElement(element);

						// Write the same text on top the old; shifted by 3 points
						element.SetTextMatrix(1, 0, 0, 1, 33, 33);
						gstate.SetFillColor(new ColorPt(0, 1, 0, 0));
						gstate.SetFillOpacity(0.5);

						writer.WriteElement(element);
						writer.WriteElement(eb.CreateTextEnd());

						// Draw three overlapping transparent circles.
						eb.PathBegin();     // start constructing the path
						eb.MoveTo(459.223, 505.646);
						eb.CurveTo(459.223, 415.841, 389.85, 343.04, 304.273, 343.04);
						eb.CurveTo(218.697, 343.04, 149.324, 415.841, 149.324, 505.646);
						eb.CurveTo(149.324, 595.45, 218.697, 668.25, 304.273, 668.25);
						eb.CurveTo(389.85, 668.25, 459.223, 595.45, 459.223, 505.646);
						element = eb.PathEnd();
						element.SetPathFill(true);

						gstate = element.GetGState();
						gstate.SetFillColorSpace(ColorSpace.CreateDeviceRGB());
						gstate.SetFillColor(new ColorPt(0, 0, 1));                     // Blue Circle

						gstate.SetBlendMode(GState.BlendMode.e_bl_normal);
						gstate.SetFillOpacity(0.5);
						writer.WriteElement(element);

						// Translate relative to the Blue Circle
						gstate.SetTransform(1, 0, 0, 1, 113, -185);
						gstate.SetFillColor(new ColorPt(0, 1, 0));                     // Green Circle
						gstate.SetFillOpacity(0.5);
						writer.WriteElement(element);

						// Translate relative to the Green Circle
						gstate.SetTransform(1, 0, 0, 1, -220, 0);
						gstate.SetFillColor(new ColorPt(1, 0, 0));                     // Red Circle
						gstate.SetFillOpacity(0.5);
						writer.WriteElement(element);

						writer.End();  // save changes to the current page
						doc.PagePushBack(page);

						// End page ------------------------------------
					}

					// Save as a linearized file which is most popular  
					// and effective format for quick PDF Viewing.
					var bytes = doc.Save(SDFDoc.SaveOptions.e_linearized);
                    return new FileContentResult(bytes, "application/pdf");
                }
            }
            catch (PDFNetException e)
            {
                log.LogInformation(e.Message);
                return new InternalServerErrorResult();
            }
        }
    }
}
