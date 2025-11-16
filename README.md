<h2>Direct implementation of SDLWindow in WinForms using FNA Platform.</h2>
<p><b>Main Features</b></p>
<ul>
<li>Full support of FNA Platform and FNA3D lib</li>
<li>Full GPU performance due to direct SDL Handle embeding</li>
<li>ContentManager is included ( Root directory is set to: Content )</li>
<li>Input events are also working fine ( Mouse + Keyboard tested )</li>
<li>FPS Counter and FPS lock</li>
<li>Powerful update/draw loop</li>
<li>Plug and Play with All in One</li>
</ul>
<br>
<p><b>How to get it run?</b></p>
<ul>
<li><b>FNA/src/FNAPlatform/FNAControl.cs</b> -> Add this file to FNA solution (version with SDL3 support)</li>
<li>Add System.Windows.Forms reference to the FNA.csproj and compile -> FNA.dll</li>
<li>That´s it! Now you can create WinForm (.NET8) application and use FNAControl abstract class.</li>
</ul>
<br>
<p>Have a fun.</p>

<img width="1050" height="575" alt="{EB5A1ADF-0967-49F3-8377-F0D24A517816}" src="https://github.com/user-attachments/assets/42e0bad3-8789-4141-b7c8-73220d216c25" />
