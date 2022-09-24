
peProgram.exe:     file format pei-i386

Disassembly of section .text:

00401000 <_foo>:
  401000:	55                   	push   %ebp
  401001:	89 e5                	mov    %esp,%ebp
  401003:	5d                   	pop    %ebp
  401004:	c3                   	ret    

00401005 <_main1>:
  401005:	55                   	push   %ebp
  401006:	89 e5                	mov    %esp,%ebp
  401008:	83 ec 04             	sub    $0x4,%esp
  40100b:	c7 45 fc 03 00 00 00 	movl   $0x3,0xfffffffc(%ebp)
  401012:	c7 45 fc 05 00 00 00 	movl   $0x5,0xfffffffc(%ebp)
  401019:	e8 e2 ff ff ff       	call   401000 <_foo>
  40101e:	b8 02 00 00 00       	mov    $0x2,%eax
  401023:	c9                   	leave  
  401024:	c3                   	ret    
  401025:	90                   	nop    
  401026:	90                   	nop    
  401027:	90                   	nop    
  401028:	90                   	nop    
  401029:	90                   	nop    
  40102a:	90                   	nop    
  40102b:	90                   	nop    
  40102c:	90                   	nop    
  40102d:	90                   	nop    
  40102e:	90                   	nop    
  40102f:	90                   	nop    

00401030 <__CTOR_LIST__>:
  401030:	ff                   	(bad)  
  401031:	ff                   	(bad)  
  401032:	ff                   	(bad)  
  401033:	ff 00                	incl   (%eax)
  401035:	00 00                	add    %al,(%eax)
	...

00401038 <__DTOR_LIST__>:
  401038:	ff                   	(bad)  
  401039:	ff                   	(bad)  
  40103a:	ff                   	(bad)  
  40103b:	ff 00                	incl   (%eax)
  40103d:	00 00                	add    %al,(%eax)
	...
