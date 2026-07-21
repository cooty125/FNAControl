<h2>Direct implementation of SDLWindow in WinForms using FNA Platform.</h2>
<p><b>Main Features</b></p>
<ul>
<li>✔ Full support of FNA Platform and FNA3D lib</li>
<li>✔ Full GPU performance due to direct SDL Handle embeding</li>
<li>✔ Embedded SDL3 window inside WinForms Control</li>
<li>✔ GraphicsDevice initialization</li>
<li>✔ Background render thread</li>
<li>✔ VSync rendering</li>
<li>✔ Keyboard & Mouse input via SDL</li>
<li>✔ Resize support</li>
<li>✔ ContentManager support</li>
<li>✔ Compatible with FNA lifecycle (Initialize/Update/Draw)</li>
<li>✔ Plug and Play with All in One</li>
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

<p>Demo project with simple defined vertex buffer cube</p>
<img width="1050" height="575" alt="{EB5A1ADF-0967-49F3-8377-F0D24A517816}" src="https://github.com/user-attachments/assets/42e0bad3-8789-4141-b7c8-73220d216c25" />
<p>Loading FBX models with textures</p>
<img width="785" height="760" alt="{B1A426A7-A3D3-4787-B8ED-60D2A43D5424}" src="https://github.com/user-attachments/assets/9f657121-5266-488e-a7f3-39573b4a7bae" />
<p>Wide range of usage (Model viewer with 8K textures)</p>
<img width="1009" height="762" alt="{B6DC0913-6806-4A57-B89A-E7F1913277B0}" src="https://github.com/user-attachments/assets/3a80374a-710a-4965-b107-87de362f8a1e" />
